using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;

namespace SertecDashboard.Api.Controllers;


[ApiController]
[Route("api")]
public class PasswordResetsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;
    private readonly EmailService _email;

    public PasswordResetsController(AppDbContext db, SseService sse, EmailService email)
    {
        _db    = db;
        _sse   = sse;
        _email = email;
    }

    // POST /api/password-reset
    [HttpPost("password-reset")]
    public async Task<IActionResult> Submit([FromBody] PasswordResetRequest body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(body.Username))
                return BadRequest(new { error = "Felhasználónév megadása kötelező" });

            var user = _db.Users
                .Include(x => x.Roles)
                .Where(u => u.Username == body.Username.Trim())
                .FirstOrDefault();
            if (user == null) return NotFound(new { error = "Felhasználó nem található" });

            // If there is already an unhandled request, return it instead of creating a duplicate.
            var existing = await _db.PasswordResets
                .FirstOrDefaultAsync(r => r.Username == body.Username && !r.Handled);

            if (existing != null)
                return Ok(new { id = existing.Id, message = "Jelszó frissítési kérelem már folyamatban van" });

            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nowStr = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");

            var role = _db.Roles
                .Where(x => x.Name == user.Roles.Name)
                .FirstOrDefault();

            var reset = new PasswordReset
            {
                Username = user.Username,
                DisplayName = user.DisplayName,
                RoleId = role.roleId,
                RequestedAt = nowStr,
                RequestedAtMs = nowMs,
                Handled = false,
            };
            _db.PasswordResets.Add(reset);

            // Keep table under 200 rows
            var count = await _db.PasswordResets.CountAsync();
            if (count >= 200)
            {
                var oldest = await _db.PasswordResets
                    .OrderBy(r => r.RequestedAtMs).Take(count - 199).ToListAsync();
                _db.PasswordResets.RemoveRange(oldest);
            }

            await _db.SaveChangesAsync();
            _sse.Broadcast("password-resets", await _db.PasswordResets.ToListAsync());

            // Notify the admin by email if an address is configured.
            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Roles.Name == "admin");
            if (!string.IsNullOrWhiteSpace(admin?.Email))
            {
                await _email.SendAsync(admin.Email,
                    $"🔑 Jelszó reset kérés — {user.DisplayName}",
                    $"Felhasználó: {user.DisplayName} ({user.Username})\n" +
                    $"Szerepe: {user.Role}\nIdő: {nowStr}\n\n" +
                    "Kérjük kezelje az admin dashboardon.");
            }

            return Ok(new { id = reset.Id, message = "Kérelem sikeresn elküldve" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Hibatörtént a kérés feldolgozása során", details = ex.Message });
        }

    }

    // GET /api/password-resets
    [HttpGet("password-resets")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _db.PasswordResets
                    .Include(x => x.Roles)
                    .Select(x => new PasswordResetDTO
                    {
                        Id = x.Id,
                        Username = x.Username,
                        DisplayName = x.DisplayName,
                        Role = x.Roles.Name,
                        RequestedAt = x.RequestedAt,
                        RequestedAtMs = x.RequestedAtMs,
                        Handled = x.Handled,
                        HandledAt = x.HandledAt
                    }).ToListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Hibatörtént a kérés feldolgozása során", details = ex.Message });
        }
    }
     

    // PATCH /api/password-resets/{id}/handle
    [HttpPatch("password-resets/{id}/handle")]
    public async Task<IActionResult> Handle(int id, [FromBody] HandleResetRequest body)
    {
        try
        {
            var reset = _db.PasswordResets
            .Where(x => x.Id == id)
            .FirstOrDefault();

            if (reset == null) return NotFound(new { error = "Nem található" });

            // Optionally set a new password while handling.
            if (!string.IsNullOrWhiteSpace(body.NewPassword))
            {
                var user = await _db.Users.FindAsync(reset.Username);
                if (user != null)
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.NewPassword.Trim());
            }

            reset.Handled = true;
            reset.HandledAt = DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss");
            await _db.SaveChangesAsync();

            _sse.Broadcast("password-resets", await _db.PasswordResets.ToListAsync());
            return Ok(reset);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Hibatörtént a kérés feldolgozása során", details = ex.Message });
        }

    }

    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}

public record PasswordResetRequest(string? Username);
public record HandleResetRequest(string? NewPassword);

public class PasswordResetDTO
{
    public int Id           { get; set; }
    public string Username     { get; set; } 
    public string DisplayName  { get; set; } 
    public string    Role         { get; set; }
    public string RequestedAt   { get; set; }
    public long   RequestedAtMs { get; set; }
    public bool   Handled      { get; set; }
    public string? HandledAt   { get; set; }
}