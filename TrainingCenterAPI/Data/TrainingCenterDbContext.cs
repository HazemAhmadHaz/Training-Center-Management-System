using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Data;

public partial class TrainingCenterDbContext : DbContext
{
    public TrainingCenterDbContext()
    {
    }

    public TrainingCenterDbContext(DbContextOptions<TrainingCenterDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Person> People { get; set; }
    public virtual DbSet<Student> Students { get; set; }
    public virtual DbSet<Instructor> Instructors { get; set; }
    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<Enrollment> Enrollments { get; set; }
    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =========================
        // Person
        // =========================
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.PersonId);

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("UQ_People_Email");

            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(e => e.Role)
                .IsRequired();
        });
        // =========================
        // Refresh Token
        // =========================

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId);


            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsRequired();


            entity.HasIndex(e => e.Token)
                .IsUnique();


            entity.HasOne(e => e.Person)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // =========================
        // Audit Log
        // =========================
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.AuditLogId);

            entity.Property(x => x.Action)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(x => x.Person)
                .WithMany()
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // =========================
        // Student
        // =========================
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId);

            entity.HasIndex(e => e.Status, "IX_Students_Status");

            entity.HasIndex(e => e.PersonId)
                .IsUnique()
                .HasDatabaseName("UQ_Students_PersonId");

            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(s => s.Person)
                .WithOne()
                .HasForeignKey<Student>(s => s.PersonId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_People");
        });

        // =========================
        // Instructor
        // =========================
        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstructorId);

            entity.HasIndex(e => e.ManagerId, "IX_Instructors_ManagerId");

            entity.HasIndex(e => e.PersonId)
                .IsUnique()
                .HasDatabaseName("UQ_Instructors_PersonId");

            entity.Property(e => e.Salary)
                .HasColumnType("decimal(10, 2)");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasOne(i => i.Person)
                .WithOne()
                .HasForeignKey<Instructor>(i => i.PersonId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Instructors_People");

            entity.HasOne(i => i.Manager)
                .WithMany(p => p.InverseManager)
                .HasForeignKey(i => i.ManagerId)
                .HasConstraintName("FK_Instructors_Manager");
        });

        // =========================
        // Admin
        // =========================
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId);

            entity.HasIndex(e => e.PersonId)
                .IsUnique()
                .HasDatabaseName("UQ_Admins_PersonId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(a => a.Person)
                .WithOne()
                .HasForeignKey<Admin>(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Admins_People");
        });

        // =========================
        // Course
        // =========================
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.InstructorId, "IX_Courses_InstructorId");

            entity.HasIndex(e => e.Status, "IX_Courses_Status");

            entity.HasIndex(e => e.Code, "UQ_Courses_Code")
                .IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(30);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)");

            entity.Property(e => e.PublishedAt)
                .HasColumnType("datetime");

            entity.Property(e => e.Title)
                .HasMaxLength(150);

            entity.HasOne(d => d.Instructor)
                .WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Instructors");
        });

        // =========================
        // Enrollment
        // =========================
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Enrollments_CourseId");

            entity.HasIndex(e => e.Status, "IX_Enrollments_Status");

            entity.HasIndex(e => e.StudentId, "IX_Enrollments_StudentId");

            entity.HasIndex(
                e => new { e.StudentId, e.CourseId },
                "UQ_Enrollments_StudentId_CourseId")
                .IsUnique();

            entity.Property(e => e.CompletionDate)
                .HasColumnType("datetime");

            entity.Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.FinalGrade)
                .HasColumnType("decimal(5, 2)");

            entity.Property(e => e.ProgressPercent)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Course)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Enrollments_Courses");

            entity.HasOne(d => d.Student)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Enrollments_Students");
        });

        // =========================
        // StudentProfile
        // =========================
        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.StudentId);

            entity.Property(e => e.StudentId)
                .ValueGeneratedNever();

            entity.Property(e => e.Address)
                .HasMaxLength(200);

            entity.Property(e => e.Bio)
                .HasMaxLength(500);

            entity.Property(e => e.City)
                .HasMaxLength(100);

            entity.Property(e => e.Country)
                .HasMaxLength(100);

            entity.Property(e => e.LinkedInUrl)
                .HasMaxLength(200);

            entity.HasOne(d => d.Student)
                .WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_StudentProfiles_Students");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}