using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Models.Entities;

namespace TestManagementApplication.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Test> Tests => Set<Test>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<TestAssignment> TestAssignments => Set<TestAssignment>();
        public DbSet<TestSession> TestSessions => Set<TestSession>();
        public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
        public DbSet<Violation> Violations => Set<Violation>();
        public DbSet<CapturedImage> CapturedImages => Set<CapturedImage>();
        public DbSet<VideoRecording> VideoRecordings => Set<VideoRecording>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ──────────────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Username).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Username).HasMaxLength(100).IsRequired();
                e.Property(u => u.Email).HasMaxLength(200).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).HasMaxLength(20).IsRequired();
            });

            // ── Test ──────────────────────────────────────────────────
            modelBuilder.Entity<Test>(e =>
            {
                e.HasKey(t => t.Id);
                e.Property(t => t.Title).HasMaxLength(300).IsRequired();
                e.HasOne(t => t.Creator)
                 .WithMany(u => u.CreatedTests)
                 .HasForeignKey(t => t.CreatedBy)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Question ──────────────────────────────────────────────
            modelBuilder.Entity<Question>(e =>
            {
                e.HasKey(q => q.Id);
                e.Property(q => q.QuestionText).IsRequired();
                e.Property(q => q.CorrectOption).HasMaxLength(1).IsRequired();
                e.HasOne(q => q.Test)
                 .WithMany(t => t.Questions)
                 .HasForeignKey(q => q.TestId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── TestAssignment ────────────────────────────────────────
            modelBuilder.Entity<TestAssignment>(e =>
            {
                e.HasKey(a => a.Id);
                e.HasIndex(a => new { a.TestId, a.UserId }).IsUnique();
                e.HasOne(a => a.Test)
                 .WithMany(t => t.TestAssignments)
                 .HasForeignKey(a => a.TestId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(a => a.User)
                 .WithMany(u => u.TestAssignments)
                 .HasForeignKey(a => a.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── TestSession ───────────────────────────────────────────
            modelBuilder.Entity<TestSession>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Status).HasMaxLength(20).IsRequired();
                e.Property(s => s.Percentage).HasPrecision(5, 2);
                e.Property(s => s.LastKnownIp).HasMaxLength(64);
                e.Property(s => s.LastKnownUserAgent).HasMaxLength(512);
                e.HasOne(s => s.Test)
                 .WithMany(t => t.TestSessions)
                 .HasForeignKey(s => s.TestId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(s => s.User)
                 .WithMany(u => u.TestSessions)
                 .HasForeignKey(s => s.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── UserAnswer ────────────────────────────────────────────
            modelBuilder.Entity<UserAnswer>(e =>
            {
                e.HasKey(a => a.Id);
                e.HasIndex(a => new { a.SessionId, a.QuestionId }).IsUnique();
                e.Property(a => a.SelectedOption).HasMaxLength(1).IsRequired();
                e.HasOne(a => a.Session)
                 .WithMany(s => s.UserAnswers)
                 .HasForeignKey(a => a.SessionId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(a => a.Question)
                 .WithMany(q => q.UserAnswers)
                 .HasForeignKey(a => a.QuestionId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Violation ─────────────────────────────────────────────
            modelBuilder.Entity<Violation>(e =>
            {
                e.HasKey(v => v.Id);
                e.Property(v => v.ViolationType).HasMaxLength(50).IsRequired();
                e.HasOne(v => v.Session)
                 .WithMany(s => s.Violations)
                 .HasForeignKey(v => v.SessionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CapturedImage ─────────────────────────────────────────
            modelBuilder.Entity<CapturedImage>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.FilePath).IsRequired();
                e.HasOne(c => c.Session)
                 .WithMany(s => s.CapturedImages)
                 .HasForeignKey(c => c.SessionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── VideoRecording ────────────────────────────────────────
            modelBuilder.Entity<VideoRecording>(e =>
            {
                e.HasKey(v => v.Id);
                e.Property(v => v.FilePath).IsRequired();
                e.Property(v => v.Type).HasMaxLength(10).IsRequired();
                e.HasOne(v => v.Session)
                 .WithMany(s => s.VideoRecordings)
                 .HasForeignKey(v => v.SessionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
