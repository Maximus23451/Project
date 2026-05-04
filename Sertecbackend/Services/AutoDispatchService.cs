using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;

namespace SertecDashboard.Api.Services;

/// <summary>
/// BackgroundService that runs a 30-second timer loop — identical in behaviour
/// to the setInterval(…, 30000) in the original Node.js server.js.
///
/// Responsibilities every tick:
///   1. Dispatch "Every X" questions when their interval has elapsed.
///   2. Auto-reset the shift clock after 12 hours (prevents questions going silent).
///   3. Expire overdue PendingItems and create missed-question alerts.
///   4. Broadcast SSE events for any changes.
///
/// Why IServiceScopeFactory?
///   AppDbContext is registered as a scoped service (one per HTTP request).
///   A BackgroundService lives as a singleton, so it must create its own scope
///   each tick to obtain a fresh DbContext — this is the official ASP.NET Core pattern.
/// </summary>
public class AutoDispatchService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SseService           _sse;
    private readonly EmailService         _email;
    private readonly ILogger<AutoDispatchService> _logger;

    private static readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public AutoDispatchService(
        IServiceScopeFactory scopeFactory,
        SseService           sse,
        EmailService         email,
        ILogger<AutoDispatchService> logger)
    {
        _scopeFactory = scopeFactory;
        _sse          = sse;
        _email        = email;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("AutoDispatchService started.");

        // Small initial delay so the database is ready before the first tick.
        await Task.Delay(TimeSpan.FromSeconds(5), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await TickAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "AutoDispatchService tick error.");
            }

            await Task.Delay(_interval, ct);
        }
    }

    // ── Main tick logic ───────────────────────────────────────────────────
    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var nowMs       = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var nowStr      = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");
        var activeShift = await db.Shifts.FirstOrDefaultAsync(s => s.Active, ct);

        bool questionsUpdated  = false;
        bool pendingBroadcast  = false;

        var questions = await db.Questions.ToListAsync(ct);

        // ── 1. Dispatch timed questions ───────────────────────────────────
        if (activeShift != null)
        {
            foreach (var q in questions)
            {
                // "Every shift" questions are dispatched only at login, not by timer.
                if (q.Freq == "Every shift") continue;
                // Setup and shift_manager_check questions are sent manually by QA.
                if (q.Type == "setup" || q.Type == "shift_manager_check") continue;

                var freqMs = GetFreqMs(q.Freq);
                if (freqMs == null) continue;

                var lastSent = q.LastShiftSentMs > 0 ? q.LastShiftSentMs : activeShift.StartTime;
                if (nowMs - lastSent < freqMs.Value) continue;

                q.LastShiftSentMs = nowMs;
                q.LastSent        = nowMs;
                questionsUpdated  = true;

                var item = CreatePendingItem(q, nowMs, nowStr, activeShift.Id, sentBy: "auto");
                await UpsertPendingItemAsync(db, item, ct);
                pendingBroadcast = true;
            }

            // ── 2. Auto-reset shift clock after 12 hours ─────────────────
            if (nowMs - activeShift.StartTime >= 12L * 60 * 60 * 1000)
            {
                activeShift.StartTime    = nowMs;
                activeShift.StartTimeStr = nowStr;
                foreach (var q in questions) q.LastShiftSentMs = nowMs;
                questionsUpdated = true;

                await db.SaveChangesAsync(ct);
                var allShifts = await db.Shifts.ToListAsync(ct);
                _sse.Broadcast("shifts", allShifts);
            }
        }
        else
        {
            // No active shift — still dispatch production questions on their interval.
            foreach (var q in questions)
            {
                if (q.Type == "setup" || q.Type == "shift_manager_check") continue;
                var freqMs = GetFreqMs(q.Freq);
                if (freqMs == null) continue;
                if (nowMs - (q.LastSent == 0 ? 0 : q.LastSent) < freqMs.Value) continue;

                q.LastSent       = nowMs;
                questionsUpdated = true;

                var item = CreatePendingItem(q, nowMs, nowStr, shiftId: null, sentBy: "auto");
                await UpsertPendingItemAsync(db, item, ct);
                pendingBroadcast = true;
            }
        }

        if (questionsUpdated) await db.SaveChangesAsync(ct);

        // ── 3. Expire overdue PendingItems ────────────────────────────────
        var overdue = await db.PendingItems
            .Where(p => !p.Expired && !p.AlertSent && p.Deadline < nowMs)
            .ToListAsync(ct);

        foreach (var p in overdue)
        {
            var answered = await db.Responses.AnyAsync(r => r.PendingId == p.Id, ct);
            p.Expired = true;

            if (!answered)
            {
                p.AlertSent = true;
                var alert = await CreateMissedAlertAsync(db, p, "missed_question", nowMs, nowStr, ct);
                await NotifyShiftManagersAsync(db, alert, ct);
            }
        }

        if (overdue.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            pendingBroadcast = true;

            var allAlerts = await db.Alerts.ToListAsync(ct);
            _sse.Broadcast("alerts", allAlerts);
        }

        // ── 4. Prune old expired items (older than 4 hours) ───────────────
        var cutoff = nowMs - 4L * 60 * 60 * 1000;
        var stale  = await db.PendingItems
            .Where(p => p.Expired && p.SentAtMs < cutoff)
            .ToListAsync(ct);

        if (stale.Count > 0)
        {
            db.PendingItems.RemoveRange(stale);
            await db.SaveChangesAsync(ct);
            pendingBroadcast = true;
        }

        // ── 5. Broadcast pending list if anything changed ─────────────────
        if (pendingBroadcast)
        {
            var allPending = await db.PendingItems.ToListAsync(ct);
            _sse.Broadcast("pending-list", allPending);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts the new PendingItem, replacing any existing item for the same
    /// (questionId + targetOperator) combination (matching original Node.js logic).
    /// </summary>
    private static async Task UpsertPendingItemAsync(AppDbContext db, PendingItem item, CancellationToken ct)
    {
        var existing = await db.PendingItems.FirstOrDefaultAsync(
            p => p.QuestionId == item.QuestionId && p.TargetOperator == item.TargetOperator, ct);

        if (existing != null) db.PendingItems.Remove(existing);
        await db.PendingItems.AddAsync(item, ct);
    }

    private static async Task<Alert> CreateMissedAlertAsync(
        AppDbContext db, PendingItem p, string alertType,
        long nowMs, string nowStr, CancellationToken ct)
    {
        var alert = new Alert
        {
            Id              = NewId(),
            Type            = alertType,
            OperatorUsername = p.TargetOperator ?? "ismeretlen",
            OperatorName    = p.TargetOperatorName ?? "Ismeretlen",
            QuestionText    = p.Text,
            PendingId       = p.Id,
            Time            = nowStr,
            TimeMs          = nowMs,
            Acknowledged    = false,
            ShiftId         = p.ShiftId,
        };
        await db.Alerts.AddAsync(alert, ct);

        // Keep alert table under 500 rows
        var count = await db.Alerts.CountAsync(ct);
        if (count > 500)
        {
            var oldest = await db.Alerts.OrderBy(a => a.TimeMs).Take(count - 500).ToListAsync(ct);
            db.Alerts.RemoveRange(oldest);
        }

        return alert;
    }

    private async Task NotifyShiftManagersAsync(AppDbContext db, Alert alert, CancellationToken ct)
    {
        var managers = await db.Users
            .Where(u => u.Roles.Name == "shift_manager" && u.Email != null)
            .ToListAsync(ct);

        foreach (var sm in managers)
        {
            await _email.SendAsync(
                sm.Email!,
                $"⚠️ Elmulasztott kérdés — {alert.OperatorName}",
                $"Operátor: {alert.OperatorName}\n" +
                $"Kérdés: {alert.QuestionText}\n" +
                $"Típus: {alert.Type}\n" +
                $"Idő: {alert.Time}");
        }
    }

    private static PendingItem CreatePendingItem(
        Question q, long nowMs, string nowStr,
        string? shiftId, string sentBy,
        string? targetOperator = null, string? targetOperatorName = null)
    {
        var answerWindowMs = q.AnswerWindowMs > 0 ? q.AnswerWindowMs : 600_000;
        return new PendingItem
        {
            Id                 = NewId(),
            QuestionId         = q.Id,
            Text               = q.Text,
            SentAt             = nowStr,
            SentAtMs           = nowMs,
            Deadline           = nowMs + answerWindowMs,
            AnswerWindowMs     = answerWindowMs,
            AlertAnswer        = q.AlertAnswer,
            TargetOperator     = targetOperator,
            TargetOperatorName = targetOperatorName,
            YesLabel           = q.YesLabel,
            NoLabel            = q.NoLabel,
            RequireExplanation = q.RequireExplanation,
            SentBy             = sentBy,
            ShiftId            = shiftId,
            AlertSent          = false,
            Expired            = false,
        };
    }

    /// <summary>Converts a question frequency string to milliseconds.</summary>
    private static long? GetFreqMs(string freq) => freq switch
    {
        "Every 30 min"  =>  30L * 60 * 1000,
        "Every 1 hour"  =>  60L * 60 * 1000,
        "Every 2 hours" => 120L * 60 * 1000,
        "Once per day"  => 1440L * 60 * 1000,
        _               => null,
    };

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}
