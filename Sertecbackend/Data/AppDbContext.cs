using Microsoft.EntityFrameworkCore;
using SertecDashboard.Api.Models;

namespace SertecDashboard.Api.Data;

/// <summary>
/// Entity Framework Core DbContext — single database for the entire application.
/// All tables are created/updated automatically via EF Core migrations.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Tables ────────────────────────────────────────────────────────────
    public DbSet<AppUser>         Users          => Set<AppUser>();
    public DbSet<Question>        Questions      => Set<Question>();
    public DbSet<PendingItem>     PendingItems   => Set<PendingItem>();
    public DbSet<QuestionResponse> Responses     => Set<QuestionResponse>();
    public DbSet<Shift>           Shifts         => Set<Shift>();
    public DbSet<Alert>           Alerts         => Set<Alert>();
    public DbSet<Machine>         Machines       => Set<Machine>();
    public DbSet<MachinePart>     MachineParts   => Set<MachinePart>();
    public DbSet<Document>        Documents      => Set<Document>();
    public DbSet<PasswordReset>   PasswordResets => Set<PasswordReset>();
    public DbSet<Roles>           Roles           => Set<Roles>();
    public DbSet<Parts>           Parts           => Set<Parts>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── AppUser ──────────────────────────────────────────────────────
        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Username);
            // Never index by password hash — it is never searched
        });

        // ── Question ─────────────────────────────────────────────────────
        modelBuilder.Entity<Question>(e =>
        {
            e.ToTable("questions");
            e.HasKey(q => q.Id);
        });

        // ── PendingItem ──────────────────────────────────────────────────
        modelBuilder.Entity<PendingItem>(e =>
        {
            e.ToTable("pendingitems");
            e.HasKey(p => p.Id);
            // Index for the expired-cleanup query
            e.HasIndex(p => p.Expired);
        });

        // ── QuestionResponse ──────────────────────────────────────────────
        modelBuilder.Entity<QuestionResponse>(e =>
        {
            e.ToTable("responses");
            e.HasKey(r => r.Id);
            // Operator is queried often for stats filtering
            e.HasIndex(r => r.Operator);
            e.HasIndex(r => r.TimeMs);
        });

        // ── Shift ────────────────────────────────────────────────────────
        modelBuilder.Entity<Shift>(e =>
        {
            e.ToTable("shifts");
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Active);
        });

        // ── Alert ────────────────────────────────────────────────────────
        modelBuilder.Entity<Alert>(e =>
        {
            e.ToTable("alerts");
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.Acknowledged);
        });

        // ── Machine / MachinePart (one-to-many) ───────────────────────────

        modelBuilder.Entity<MachinePart>(e =>
        {
            e.ToTable("machineparts");
            e.HasKey(p => new { p.MachineId, p.PartId });
        });

        // ── Document ─────────────────────────────────────────────────────
        modelBuilder.Entity<Document>(e =>
        {
            e.ToTable("documents");
            e.HasKey(d => d.Id);
            // Data stores a file path (/docs/xxx.pdf) or a cloud embed URL — nvarchar(max) by default.
        });

        // ── PasswordReset ─────────────────────────────────────────────────
        modelBuilder.Entity<PasswordReset>(e =>
        {
            e.ToTable("passwordresets");
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Handled);
        });
    }
}
