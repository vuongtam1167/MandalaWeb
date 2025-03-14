using MandalaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MandalaApp.Data
{
    public class MandalaAppDbContext : DbContext
    {
        public MandalaAppDbContext(DbContextOptions<MandalaAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Mandala> Mandalas { get; set; }
        public DbSet<MandalaTarget> MandalaTargets { get; set; }
        public DbSet<MandalaDetail> MandalaDetails { get; set; }

        // Override OnModelCreating để cấu hình các entity bằng Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ví dụ: cấu hình tên bảng cho Mandala
            modelBuilder.Entity<Mandala>()
                .ToTable("Mandala");

            // Cấu hình khóa chính phức hợp cho MandalaTarget
            modelBuilder.Entity<MandalaTarget>()
                .HasKey(mt => new { mt.MandalaLv, mt.MandalaID });
        }
    }
}
