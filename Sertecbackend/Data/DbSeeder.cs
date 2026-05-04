using SertecDashboard.Api.Controllers;
using SertecDashboard.Api.Models;

namespace SertecDashboard.Api.Data;

/// <summary>
/// Seeds the database with default users and starter questions on the very first run.
/// Safe to call on every startup — it checks for existing data before inserting.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedUsersAsync(db);
        await SeedQuestionsAsync(db);
    }

    // ── Users ─────────────────────────────────────────────────────────────
    private static async Task SeedUsersAsync(AppDbContext db)
    {
        // Ensure the admin account always exists with the correct role.
        var admin = db.Users.FirstOrDefault(u => u.Username == "admin");
        if (admin == null)
        {
            var role=db.Roles
                .Where(x=>x.Name=="admin")
                .Select(x=>x.roleId)
                .FirstOrDefault();

            db.Users.Add(new AppUser
            {
                Username     = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role         = role,
                DisplayName  = "Rendszergazda",
                CreatedAt    = DateTime.UtcNow,
            });
        }
        

        // Only seed demo users when the table is brand-new (just the admin row or empty).
        if (db.Users.Count() <= 1)
        {
            var demos = new[]
            {
                ("manager1",  "demo123", "management",    "Kovács Péter"),
                ("manager2",  "demo123", "management",    "Nagy Anna"),
                ("qa1",       "demo123", "qa",            "Szabó Gábor"),
                ("qa2",       "demo123", "qa",            "Tóth Eszter"),
                ("operator1", "demo123", "operator",      "Horváth Béla"),
                ("operator2", "demo123", "operator",      "Varga Zsolt"),
                ("operator3", "demo123", "operator",      "Kiss Mónika"),
                ("setter1",   "demo123", "setter",        "Fekete Imre"),
                ("setter2",   "demo123", "setter",        "Balogh Krisztián"),
                ("shift1",    "demo123", "shift_manager", "Molnár László"),
                ("shift2",    "demo123", "shift_manager", "Pap Erzsébet"),
            };

            foreach (var (uname, pwd, role, display) in demos)
            {
                var Initrole=db.Roles
                    .Where(x=>x.Name==role)
                    .Select(x=>x.roleId)
                    .FirstOrDefault();

                if (!db.Users.Any(u => u.Username == uname))
                {
                    db.Users.Add(new AppUser
                    {
                        Username = uname,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(pwd),
                        Role = Initrole,
                        DisplayName = display,
                        CreatedAt = DateTime.UtcNow,
                        RFID = (role=="operator" || role=="shift_manager" || role == "setter") ? UsersController.GenerateRandomRFID() : null
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    // ── Questions ─────────────────────────────────────────────────────────
    private static async Task SeedQuestionsAsync(AppDbContext db)
    {
        // Only seed when the question bank is completely empty.
        if (db.Questions.Any()) return;

        var createdAt = DateTime.UtcNow.ToString("yyyy.MM.dd. HH:mm:ss");

        var questions = new[]
        {
            new Question { Id = NewId(), Text = "Minden gép megfelelően működik?",        Freq = "Every 1 hour",  Type = "production",          AlertAnswer = "no",  YesLabel = "Igen",    NoLabel = "Nem",      RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "Elvégezted a biztonsági ellenőrzést?",   Freq = "Every shift",   Type = "production",          AlertAnswer = "no",  YesLabel = "Igen",    NoLabel = "Nem",      RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "A munkaterület tiszta és rendezett?",    Freq = "Every 2 hours", Type = "production",          AlertAnswer = "no",  YesLabel = "Igen",    NoLabel = "Nem",      RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "Van aktív minőségi probléma?",           Freq = "Every 30 min",  Type = "production",          AlertAnswer = "yes", YesLabel = "Igen",    NoLabel = "Nem",      RequireExplanation = "yes", AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "A gép kalibrálása megfelelő?",           Freq = "Every shift",   Type = "setup",               AlertAnswer = "no",  YesLabel = "Rendben", NoLabel = "Probléma", RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "Az összes szerszám a helyén van?",       Freq = "Every shift",   Type = "setup",               AlertAnswer = "no",  YesLabel = "Rendben", NoLabel = "Hiány",    RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "A biztonsági berendezések ellenőrizve?", Freq = "Every shift",   Type = "setup",               AlertAnswer = "no",  YesLabel = "Rendben", NoLabel = "Probléma", RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "A műszak normálisan halad?",             Freq = "Every 1 hour",  Type = "shift_manager_check", AlertAnswer = "no",  YesLabel = "Igen",    NoLabel = "Nem",      RequireExplanation = "no",  AnswerWindowMs = 600000, CreatedAt = createdAt },
            new Question { Id = NewId(), Text = "Van eszkalációra szoruló esemény?",      Freq = "Every 2 hours", Type = "shift_manager_check", AlertAnswer = "yes", YesLabel = "Igen",    NoLabel = "Nem",      RequireExplanation = "yes", AnswerWindowMs = 600000, CreatedAt = createdAt },
        };

        db.Questions.AddRange(questions);
        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Generates a 16-character hex ID (same length as the Node.js uid()).</summary>
    private static string NewId() =>
        Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(8))
               .ToLowerInvariant();
}
