using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/pending")]
public class PendingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    public PendingController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/pending
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _db.PendingItems.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Függőben lévő kérdések lekérése sikertelen", details = ex.Message });
        }

    }


    // POST /api/pending
    [HttpPost]
    public async Task<IActionResult> Dispatch([FromBody] DispatchRequest body)
    {
        try
        {
            var ids = new List<string>(body.QuestionIds ?? []);
            if (!string.IsNullOrWhiteSpace(body.QuestionId) && !ids.Contains(body.QuestionId))
                ids.Add(body.QuestionId);

            if (ids.Count == 0)
                return BadRequest(new { error = "Kérdés azonosítók szükségesek" });

            AppUser? targetUser = null;
            if (!string.IsNullOrWhiteSpace(body.TargetOperator))
                targetUser = await _db.Users.FindAsync(body.TargetOperator);

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nowStr = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");

            var activeShift = await _db.Shifts.FirstOrDefaultAsync(s => s.Active);

            var added = new List<PendingItem>();

            foreach (var qid in ids)
            {
                var q = await _db.Questions.FindAsync(qid);
                if (q == null) continue;

                q.LastSent = nowMs;

                var answerWindowMs = q.AnswerWindowMs > 0 ? q.AnswerWindowMs : 600_000;
                var item = new PendingItem
                {
                    Id = NewId(),
                    QuestionId = q.Id,
                    Text = q.Text,
                    SentAt = nowStr,
                    SentAtMs = nowMs,
                    Deadline = nowMs + answerWindowMs,
                    AnswerWindowMs = answerWindowMs,
                    AlertAnswer = q.AlertAnswer,
                    TargetOperator = targetUser?.Username,
                    TargetOperatorName = targetUser?.DisplayName,
                    YesLabel = q.YesLabel,
                    NoLabel = q.NoLabel,
                    RequireExplanation = q.RequireExplanation,
                    SentBy = "qa",
                    ShiftId = activeShift?.Id,
                };

                var existing = await _db.PendingItems.FirstOrDefaultAsync(
                    p => p.QuestionId == item.QuestionId && p.TargetOperator == item.TargetOperator);
                if (existing != null) _db.PendingItems.Remove(existing);

                _db.PendingItems.Add(item);
                added.Add(item);
            }

            if (added.Count == 0)
                return NotFound(new { error = "Érvényes kérdések nem találhatók" });

            await _db.SaveChangesAsync();

            var all = await _db.PendingItems.ToListAsync();
            _sse.Broadcast("pending-list", all);
            _sse.Broadcast("pending", added.Last());

            return Ok(added);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Kérdések kiküldése sikertelen", details = ex.Message });
        }


    }

    // DELETE /api/pending
    [HttpDelete]
    public async Task<IActionResult> ClearAll()
    {
        try
        {
            _db.PendingItems.RemoveRange(await _db.PendingItems.ToListAsync());
            await _db.SaveChangesAsync();

            _sse.Broadcast("pending-list", new List<object>());
            _sse.Broadcast("pending", null!);
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Függőben lévő kérdések törlése sikertelen", details = ex.Message });
        }

    }

    // DELETE /api/pending/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOne(string id)
    {
        try
        {
            var item = await _db.PendingItems.FindAsync(id);
            if (item != null)
            {
                _db.PendingItems.Remove(item);
                await _db.SaveChangesAsync();
            }

            var all = await _db.PendingItems.ToListAsync();
            _sse.Broadcast("pending-list", all);
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Függőben lévő kérdés törlése sikertelen", details = ex.Message });
        }

    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record DispatchRequest(string[]? QuestionIds, string? QuestionId, string? TargetOperator);
