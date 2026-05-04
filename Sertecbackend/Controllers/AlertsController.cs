using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;
    private readonly EmailService _email;

    public AlertsController(AppDbContext db, SseService sse, EmailService email)
    {
        _db    = db;
        _sse   = sse;
        _email = email;
    }

    // GET /api/alerts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _db.Alerts.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Értesítések lekérése sikertelen", details = ex.Message });
        }

    }


    // POST /api/alerts
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlertRequest body)
    {
        try
        {
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nowStr = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");

            var activeShift = await _db.Shifts.FirstOrDefaultAsync(s => s.Active);

            PendingItem? pending = null;
            if (!string.IsNullOrWhiteSpace(body.PendingId))
            {
                pending = await _db.PendingItems.FindAsync(body.PendingId);
                if (pending != null)
                {
                    pending.AlertSent = true;
                    pending.Expired = true;
                }
            }

            var alert = new Alert
            {
                Id = NewId(),
                Type = body.Type ?? "missed_popup",
                OperatorUsername = body.OperatorUsername ?? "ismeretlen",
                OperatorName = body.OperatorName ?? "Ismeretlen",
                QuestionText = pending?.Text ?? "Ismeretlen kérdés",
                PendingId = body.PendingId,
                Time = nowStr,
                TimeMs = nowMs,
                Acknowledged = false,
                ShiftId = pending?.ShiftId ?? activeShift?.Id,
            };
            _db.Alerts.Add(alert);
            await _db.SaveChangesAsync();

            var managers = await _db.Users
                .Where(u => u.Roles.Name == "shift_manager" && u.Email != null)
                .ToListAsync();

            foreach (var sm in managers)
                await _email.SendAsync(sm.Email!,
                    $"⚠️ Elmulasztott kérdés — {alert.OperatorName}",
                    $"Operátor: {alert.OperatorName}\nKérdés: {alert.QuestionText}\n" +
                    $"Típus: {alert.Type}\nIdő: {alert.Time}");

            _sse.Broadcast("alerts", await _db.Alerts.ToListAsync());
            return Ok(alert);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Értesítés létrehozása sikertelen", details = ex.Message });
        }   
    }

    // PATCH /api/alerts/{id}/ack
    [HttpPatch("{id}/ack")]
    public async Task<IActionResult> Acknowledge(string id)
    {
        try
        {
            var alert = await _db.Alerts.FindAsync(id);
            if (alert == null) return NotFound(new { error = "Értesítés nem található" });

            alert.Acknowledged = true;
            await _db.SaveChangesAsync();

            _sse.Broadcast("alerts", await _db.Alerts.ToListAsync());
            return Ok(alert);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Értesítés kezelése sikertelen", details = ex.Message });
        }
    }

    // DELETE /api/alerts 
    [HttpDelete]
    public async Task<IActionResult> ClearAcknowledged()
    {
        try
        {
            var acked = await _db.Alerts.Where(a => a.Acknowledged).ToListAsync();
            _db.Alerts.RemoveRange(acked);
            await _db.SaveChangesAsync();

            _sse.Broadcast("alerts", await _db.Alerts.ToListAsync());
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Értesítések törlése sikertelen", details = ex.Message });
        }

    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record CreateAlertRequest(
    string? PendingId, string? OperatorUsername, string? OperatorName, string? Type);
