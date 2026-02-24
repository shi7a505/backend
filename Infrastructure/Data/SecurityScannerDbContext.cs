using Core.Entities;
using Core.Entities.ChatBot;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SecurityScannerDbContext : DbContext
{
    public SecurityScannerDbContext(DbContextOptions<SecurityScannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; } // NEW
    public DbSet<Scan> Scans { get; set; }
    public DbSet<Vulnerability> Vulnerabilities { get; set; }
    public DbSet<Report> Reports { get; set; }

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20).HasDefaultValue("User");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Profile Configuration (NEW)
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.FullName).HasMaxLength(150);
            entity.Property(p => p.PhoneNumber).HasMaxLength(30);
            entity.Property(p => p.Address).HasMaxLength(300);
            entity.Property(p => p.Bio).HasMaxLength(2000);
            entity.Property(p => p.AvatarUrl).HasMaxLength(500);

            entity.Property(p => p.CreatedAtUtc).IsRequired();

            // 1:1 (UserId unique)
            entity.HasIndex(p => p.UserId).IsUnique();

            entity.HasOne(p => p.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<Profile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Soft delete filter
            entity.HasQueryFilter(p => p.DeletedAtUtc == null);
        });

        // Scan Configuration
        modelBuilder.Entity<Scan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TargetURL).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Scans)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Vulnerability Configuration
        modelBuilder.Entity<Vulnerability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Recommendation).HasMaxLength(1000);

            entity.HasOne(e => e.Scan)
                  .WithMany(s => s.Vulnerabilities)
                  .HasForeignKey(e => e.ScanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ScanId);
            entity.HasIndex(e => e.Severity);
        });

        // Report Configuration
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(10);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);

            entity.HasOne(e => e.Scan)
                  .WithMany(s => s.Reports)
                  .HasForeignKey(e => e.ScanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ScanId);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.UserId)
                  .IsRequired()
                  .HasMaxLength(64);

            entity.Property(c => c.CreatedAtUtc).IsRequired();
            entity.Property(c => c.UpdatedAtUtc).IsRequired();

            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.UpdatedAtUtc);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Content)
                  .IsRequired()
                  .HasMaxLength(4000);

            entity.Property(m => m.Sender)
                  .IsRequired();

            entity.Property(m => m.CreatedAtUtc).IsRequired();

            entity.HasOne(m => m.Conversation)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => m.ConversationId);
            entity.HasIndex(m => m.CreatedAtUtc);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TokenHash)
                  .IsRequired()
                  .HasMaxLength(64);

            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.ExpiresAtUtc).IsRequired();

            entity.HasIndex(x => new { x.UserId, x.TokenHash });

            entity.HasOne(x => x.User)
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}