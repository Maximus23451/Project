using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatsController(AppDbContext db) => _db = db;

    // GET /api/stats
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var activeShift = await _db.Shifts.FirstOrDefaultAsync(s => s.Active);
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            return Ok(new
            {
                questions = await _db.Questions.CountAsync(),
                responses = await _db.Responses.CountAsync(),
                noAnswers = await _db.Responses.CountAsync(r => r.Answer == "no"),
                docs = await _db.Documents.CountAsync(),
                machines = await _db.Machines.CountAsync(),
                hasPending = await _db.PendingItems.AnyAsync(p => !p.Expired),
                pendingCount = await _db.PendingItems.CountAsync(p => !p.Expired),
                alertCount = await _db.Alerts.CountAsync(a => !a.Acknowledged),
                pendingResetCount = await _db.PasswordResets.CountAsync(r => !r.Handled),
                activeShift = activeShift == null ? null : (object)new
                {
                    operatorName = activeShift.OperatorName,
                    startTimeStr = activeShift.StartTimeStr,
                    durationMs = nowMs - activeShift.StartTime,
                },
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Hiba történt a statisztikák lekérése során", details = ex.Message });

        }
    }
}
