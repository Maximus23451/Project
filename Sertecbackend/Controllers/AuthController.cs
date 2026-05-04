using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;

public class userGetDTO
{
    public string? Username { get; set; }
    public string Role { get; set; }
    public string DisplayName { get; set; }
    public string? PasswordHash { get; set; }
    public string? rfid { get; set; }
}


[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext db, SseService sse, ILogger<AuthController> logger)
    {
        _db     = db;
        _sse    = sse;
        _logger = logger;
    }


    [HttpPost("login")]
    [EnableRateLimiting("login")]   
    public async Task<IActionResult> Login([FromBody] LoginRequest body)
    {
        try
        {
            if ((string.IsNullOrWhiteSpace(body.Username) && string.IsNullOrWhiteSpace(body.Password)) && string.IsNullOrWhiteSpace(body.rfid))
                return BadRequest(new { error = "Felhasználónév és jelszó megadása kötelező" });

            userGetDTO user = null;

            if (string.IsNullOrWhiteSpace(body.rfid) && body.Username!=null && body.Password!=null)
            {
                 user= await _db.Users
                .Include(x => x.Roles)
                .Select(x => new userGetDTO
                {
                    Username = x.Username,
                    Role = x.Roles.Name,
                    DisplayName = x.DisplayName,
                    PasswordHash = x.PasswordHash
                })
                .FirstOrDefaultAsync(u => u.Username == body.Username);
            }
            else if(body.rfid!=null)
            {
                user = await _db.Users
                .Include(x => x.Roles)
                .Select(x => new userGetDTO
                {
                   Username = x.Username,
                   Role = x.Roles.Name,
                   DisplayName = x.DisplayName,
                   PasswordHash = x.PasswordHash,
                   rfid = x.RFID

                })
                .FirstOrDefaultAsync(u => u.rfid == body.rfid);
            }


            if (user == null)
                return Unauthorized(new { error = "Érvénytelen felhasználónév vagy jelszó" });

            // Jelszó-ellenőrzés csak felhasználónév+jelszó esetén
            if (!string.IsNullOrWhiteSpace(body.Username) && !string.IsNullOrWhiteSpace(body.Password))
            {
                if (!BCrypt.Net.BCrypt.Verify(body.Password, user.PasswordHash))
                    return Unauthorized(new { error = "Érvénytelen felhasználónév vagy jelszó" });
            }
            // RFID-vel autentikáció elég a user megtalálása
            if (user.Role == "operator" || user.Role == "setter")
            {
                var active = _db.Shifts
                    .Where(x => x.Active == true)
                    .FirstOrDefault();


                if (active == null)
                {
                    var shift = await StartShiftAsync(user);




                    if (user.Role == "operator")
                    {
                        var shiftQs = await _db.Questions
                            .Where(q => q.Freq == "Every shift" && q.Type == "production")
                            .ToListAsync();

                        foreach (var q in shiftQs)
                        {
                            q.LastSent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            await UpsertPendingAsync(CreatePendingItem(q, shift.Id, "auto"));
                        }
                        await _db.SaveChangesAsync();
                    }


                    if (user.Role == "setter")
                    {
                        var setupQs = await _db.Questions
                            .Where(q => q.Type == "setup")
                            .ToListAsync();

                        foreach (var q in setupQs)
                        {
                            q.LastSent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            await UpsertPendingAsync(CreatePendingItem(q, shift.Id, "auto",
                                targetOperator: user.Username, targetOperatorName: user.DisplayName));
                        }
                        await _db.SaveChangesAsync();
                    }


                    var allPending = await _db.PendingItems.ToListAsync();
                    _sse.Broadcast("pending-list", allPending);
                }

                _logger.LogInformation("User {Username} ({Role}) logged in.", user.Username, user.Role);
            }
            return Ok(new
            {
                username = user.Username,
                role = user.Role,
                displayName = user.DisplayName,
            });

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Sikertelen bejelentkezés" });


        }
    }


    private async Task<Shift> StartShiftAsync(userGetDTO user)
    {
        try
        {
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nowStr = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");

            var open = await _db.Shifts
                .Where(s => s.OperatorUsername == user.Username && s.Active)
                .ToListAsync();

            foreach (var s in open)
            {
                s.Active = false;
                s.EndTime = nowMs;
                s.EndTimeStr = nowStr;
            }

            var shift = new Shift
            {
                Id = NewId(),
                OperatorUsername = user.Username,
                OperatorName = user.DisplayName,
                Role = user.Role,
                StartTime = nowMs,
                StartTimeStr = nowStr,
                Active = true,
            };
            _db.Shifts.Add(shift);


            var questions = await _db.Questions.ToListAsync();
            foreach (var q in questions) q.LastShiftSentMs = nowMs;

            await _db.SaveChangesAsync();

            var allShifts = await _db.Shifts.ToListAsync();
            _sse.Broadcast("shifts", allShifts);

            return shift;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Műszak megkezdése sikertelen!", user.Username);
            throw;  
        }

    }

    private async Task UpsertPendingAsync(PendingItem item)
    {
        try
        {
            var existing = await _db.PendingItems.FirstOrDefaultAsync(
            p => p.QuestionId == item.QuestionId && p.TargetOperator == item.TargetOperator);

            if (existing != null) _db.PendingItems.Remove(existing);
            _db.PendingItems.Add(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pending item upsert failed for QuestionId {QuestionId} and TargetOperator {TargetOperator}",
                item.QuestionId, item.TargetOperator);
            throw;
        }

    }

    private static PendingItem CreatePendingItem(
        Question q, string shiftId, string sentBy,
        string? targetOperator = null, string? targetOperatorName = null)
    {

        try
        {
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var answerWindowMs = q.AnswerWindowMs > 0 ? q.AnswerWindowMs : 600_000;
            return new PendingItem
            {
                Id = NewId(),
                QuestionId = q.Id,
                Text = q.Text,
                SentAt = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss"),
                SentAtMs = nowMs,
                Deadline = nowMs + answerWindowMs,
                AnswerWindowMs = answerWindowMs,
                AlertAnswer = q.AlertAnswer,
                TargetOperator = targetOperator,
                TargetOperatorName = targetOperatorName,
                YesLabel = q.YesLabel,
                NoLabel = q.NoLabel,
                RequireExplanation = q.RequireExplanation,
                SentBy = sentBy,
                ShiftId = shiftId,
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Kérdés hozzáadása sikertelen!", ex);

        }
    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record LoginRequest(string? Username, string? Password, string? rfid);
