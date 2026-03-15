using Microsoft.EntityFrameworkCore;
using Sertec.Models;



namespace Sertec.Data
{
    public class Appdbcontext : DbContext
    {
        public DbSet<Roles> roles { get; set; }
        public DbSet<Users> users { get; set; }
        public DbSet<Machines> machines { get; set; }
        public DbSet<Parts> parts { get; set; }
        public DbSet<MachineParts> machineParts { get; set; }
        public DbSet<Questions> questions { get; set; }

        public Appdbcontext(DbContextOptions<Appdbcontext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Only call this ONCE at the top

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.ClrType.Name);
            }

            modelBuilder.Entity<MachineParts>()
                .HasKey(mp => new { mp.MachineId, mp.PartId });

            modelBuilder.Entity<ShiftParts>()
                .HasKey(sp => new { sp.shiftId, sp.partId });

            modelBuilder.Entity<UserShifts>()
                .HasKey(us => new { us.userId, us.shiftId });

        }
    }
}
