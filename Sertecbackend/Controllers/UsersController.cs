using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using SertecDashboard.Api.Data;
using SertecDashboard.Api.Models;
using SertecDashboard.Api.Services;
using System.Xml;

namespace SertecDashboard.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SseService   _sse;

    public UsersController(AppDbContext db, SseService sse)
    {
        _db  = db;
        _sse = sse;
    }

    // GET /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _db.Users
            .Select(u => new { u.Username, u.DisplayName, Role = u.Roles.Name, email = u.Email ?? "" })
            .ToListAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Felhasználók lekérése sikertelen", details = ex.Message });
        }
    }

    [HttpGet]
    [Route("rfid")]
    public async Task<IActionResult> GetRFID_Users()
    {
        try
        {
            var users = await _db.Users
            .Select(u => new { u.Username, u.DisplayName, Role = u.Roles.Name, email = u.Email ?? "", u.RFID })
            .Where(x => x.RFID != null)
            .GroupBy(x => x.Role)
            .ToListAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Felhasználók lekérése sikertelen", details = ex.Message });
        }
    }


    // POST /api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(body.Username) ||
            string.IsNullOrWhiteSpace(body.Password) ||
            string.IsNullOrWhiteSpace(body.Role))
                return BadRequest(new { error = "Felhasználónév, jelszó és szerepkör megadása kötelező" });

            var uname = body.Username.Trim();
            if (await _db.Users.AnyAsync(u => u.Username == uname))
                return Conflict(new { error = "A felhasználónév már létezik" });
            var role = _db.Roles
                .Where(x => x.Name == body.Role)
                .FirstOrDefault();


            var rfid = "";
            if (role.Name=="operator" || role.Name=="setter" || role.Name=="shift_manager")
            {
                 rfid = GenerateRandomRFID();
            }


            var user = new AppUser
            {
                Username = uname,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password.Trim()),
                Role = role.roleId,
                DisplayName = body.DisplayName?.Trim() ?? uname,
                Email = body.Email?.Trim() ?? null,
                CreatedAt = DateTime.UtcNow,
                RFID = rfid=="" ? null : rfid
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await BroadcastUsersAsync();
            return Ok(new { user.Username, user.DisplayName, user.Role });
        }

        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Felhasználó létrehozása sikertelen", details = ex.Message });
        }
    }

    


    // PATCH /api/users/{username}
    [HttpPatch("{username}")]
    public async Task<IActionResult> Update(string username, [FromBody] UpdateUserRequest body)
    {
        try
        {
            var user = await _db.Users
                .Include(x=>x.Roles)
                .Where(x=>x.Username==username)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound(new { error = "Felhasználó nem található" });

            if (!string.IsNullOrWhiteSpace(body.DisplayName))
                user.DisplayName = body.DisplayName.Trim();

            if (body.Email is not null)
                user.Email = string.IsNullOrWhiteSpace(body.Email) ? null : body.Email.Trim();

            if (!string.IsNullOrWhiteSpace(body.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password.Trim());

            // The admin's role can never be demoted.
            if (!string.IsNullOrWhiteSpace(body.Role) && user.Username != "admin")
            {

                var role= await _db.Roles
                    .Where(x => x.Name == body.Role.Trim())
                    .Select(x => x.roleId)
                    .FirstOrDefaultAsync();

                user.Role = role;
            }
               


            await _db.SaveChangesAsync();
            await BroadcastUsersAsync();
            return Ok(new { user.Username, user.DisplayName, user.Role });
        }

        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Felhasználó frissítése sikertelen", details = ex.Message });
        }


    }

    // DELETE /api/users/{username}
    [HttpDelete("{username}")]
    public async Task<IActionResult> Delete(string username)
    {
        try
        {
            if (username == "admin")
                return StatusCode(403, new { error = "Admin felhasználó nem törölhető" });

            var user = await _db.Users.FindAsync(username);
            if (user == null) return NotFound(new { error = "Felhasználó nem található" });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            await BroadcastUsersAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Felhasználó törlése sikertelen", details = ex.Message });
        }

    }

    private async Task BroadcastUsersAsync()
    {
        var list = await _db.Users
            .Select(u => new { u.Username, u.DisplayName, u.Role, email = u.Email ?? "" })
            .ToListAsync();
        _sse.Broadcast("users", list);
    }


    public static string GenerateRandomRFID()
    {
        var bytes = new byte[8];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public record CreateUserRequest(string Username, string Password, string Role, string? DisplayName, string? Email);
public record UpdateUserRequest(string? DisplayName, string? Email, string? Password, string? Role);