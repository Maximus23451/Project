using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/shifts")]
public class ShiftsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    public ShiftsController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/shifts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {

            return Ok(await _db.Shifts.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Műszakok lekérése sikertelen", details = ex.Message });
        }
    }


    // POST /api/shifts/end
    [HttpPost("end")]
    public async Task<IActionResult> EndShift([FromBody] EndShiftRequest body)
    {
        try
        {
            var shift = !string.IsNullOrWhiteSpace(body.ShiftId)
            ? await _db.Shifts.FindAsync(body.ShiftId)
            : await _db.Shifts.FirstOrDefaultAsync(s => s.Active);

            if (shift == null)
                return NotFound(new { error = "Nem található aktív műszak" });

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nowStr = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");

            shift.Active = false;
            shift.EndTime = nowMs;
            shift.EndTimeStr = nowStr;
            shift.EndedBy = body.EndedBy ?? "shift_manager";

            if (!string.IsNullOrWhiteSpace(body.Report))
                shift.Report = body.Report;

            await _db.SaveChangesAsync();

            var allShifts = await _db.Shifts.ToListAsync();
            _sse.Broadcast("shifts", allShifts);
            return Ok(shift);
        }

        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Műszak lezárása sikertelen", details = ex.Message });

        }
    }
}

public record EndShiftRequest(string? ShiftId, string? EndedBy, string? Report);
