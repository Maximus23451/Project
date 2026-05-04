using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/responses")]
public class ResponsesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    public ResponsesController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/responses
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        try
        {
            return Ok(await _db.Responses.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Válaszok lekérése sikertelen", details = ex.Message });
        }
    }

    // POST /api/responses
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitResponseRequest body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(body.Question) || string.IsNullOrWhiteSpace(body.Answer))
                return BadRequest(new { error = "Kérdés és válasz megadása kötelező" });

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nowStr = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");

            var response = new QuestionResponse
            {
                Id = NewId(),
                Question = body.Question,
                Answer = body.Answer,
                Reason = body.Reason ?? "",
                OperatorName = body.OperatorName ?? "Operator",
                Operator = body.Operator,               
                AlertAnswer = body.AlertAnswer ?? "no",
                Time = nowStr,
                TimeMs = nowMs,
                PendingId = body.PendingId,
            };
            _db.Responses.Add(response);

            if (!string.IsNullOrWhiteSpace(body.PendingId))
            {
                var pending = await _db.PendingItems.FindAsync(body.PendingId);
                if (pending != null) pending.Expired = true;
            }

            await _db.SaveChangesAsync();

            _sse.Broadcast("responses", await _db.Responses.ToListAsync());
            return Ok(response);
        }

        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Válasz mentése sikertelen", details = ex.Message });
        }

    }

    // DELETE /api/responses
    [HttpDelete]
    public async Task<IActionResult> ClearAll()
    {
        try
        {
            _db.Responses.RemoveRange(await _db.Responses.ToListAsync());
            await _db.SaveChangesAsync();

            _sse.Broadcast("responses", new List<object>());
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Válaszok törlése sikertelen", details = ex.Message });
        }

    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record SubmitResponseRequest(
    string? Question, string? Answer, string? Reason,
    string? OperatorName, string? Operator,
    string? PendingId, string? AlertAnswer);
