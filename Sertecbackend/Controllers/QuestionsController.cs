using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    public QuestionsController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/questions
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _db.Questions.ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Kérdések lekérése sikertelen", details = ex.Message });
        }
    }


    // POST /api/questions
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] QuestionBody body)
    {

        try
        {
            if (string.IsNullOrWhiteSpace(body.Text))
                return BadRequest(new { error = "Kérdés szövegének megadása kötelező" });

            var q = new Question
            {
                Id = NewId(),
                Text = body.Text.Trim(),
                Freq = body.Freq ?? "Every 1 hour",
                Type = body.Type ?? "production",
                AlertAnswer = body.AlertAnswer ?? "no",
                YesLabel = body.YesLabel ?? "Igen",
                NoLabel = body.NoLabel ?? "Nem",
                RequireExplanation = body.RequireExplanation ?? "no",
                AnswerWindowMs = body.AnswerWindowMs > 0 ? body.AnswerWindowMs : 600_000,
                CreatedAt = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss"),
                LastSent = 0,
                LastShiftSentMs = 0,
            };
            _db.Questions.Add(q);
            await _db.SaveChangesAsync();

            _sse.Broadcast("questions", await _db.Questions.ToListAsync());
            return Ok(q);
        }

        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Kérdés létrehozása sikertelen", details = ex.Message });
        }

    }

    // PATCH /api/questions/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] QuestionBody body)
    {
        try
        {
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return NotFound(new { error = "Kérdés nem található" });

            if (!string.IsNullOrWhiteSpace(body.Text)) q.Text = body.Text.Trim();
            if (!string.IsNullOrWhiteSpace(body.Freq)) q.Freq = body.Freq;
            if (!string.IsNullOrWhiteSpace(body.Type)) q.Type = body.Type;
            if (!string.IsNullOrWhiteSpace(body.AlertAnswer)) q.AlertAnswer = body.AlertAnswer;
            if (body.YesLabel is not null) q.YesLabel = body.YesLabel;
            if (body.NoLabel is not null) q.NoLabel = body.NoLabel;
            if (!string.IsNullOrWhiteSpace(body.RequireExplanation)) q.RequireExplanation = body.RequireExplanation;
            if (body.AnswerWindowMs > 0) q.AnswerWindowMs = body.AnswerWindowMs;

            await _db.SaveChangesAsync();
            _sse.Broadcast("questions", await _db.Questions.ToListAsync());
            return Ok(q);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Kérdés frissítése sikertelen", details = ex.Message });
        }

    }

    // DELETE /api/questions/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {

        try
        {
            var q = await _db.Questions.FindAsync(id);
            if (q == null) return NotFound(new { error = "Kérdés nem található" });

            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();

            _sse.Broadcast("questions", await _db.Questions.ToListAsync());
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Kérdés törlése sikertelen", details = ex.Message });
        }
    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record QuestionBody(
    string? Text, string? Freq, string? Type, string? AlertAnswer,
    string? YesLabel, string? NoLabel, string? RequireExplanation, long AnswerWindowMs);
