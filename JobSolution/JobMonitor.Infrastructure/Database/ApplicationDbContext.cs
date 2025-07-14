using JobMonitor.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobMonitor.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<HhTokenEntity> HhTokens { get; set; } = null!;
    public DbSet<AppTokenEntity> AppTokens { get; set; } = null!;
    public DbSet<ActiveJobEntity> ActiveJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.HhId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<HhTokenEntity>(entity =>
        {
            entity.ToTable("hh_tokens");
            entity.HasKey(e => e.UserId);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.HhToken)
                  .HasForeignKey<HhTokenEntity>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppTokenEntity>(entity =>
        {
            entity.ToTable("app_tokens");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.AppTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActiveJobEntity>(entity =>
        {
            entity.ToTable("active_jobs", "hangfire");
            entity.HasKey(a => a.SearchId);
        });
    }
}
