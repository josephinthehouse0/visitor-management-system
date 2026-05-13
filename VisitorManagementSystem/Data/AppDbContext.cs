using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.Services;

namespace VisitorManagementSystem.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<BlacklistEntry> BlacklistEntries => Set<BlacklistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var statusConverter = new EnumToStringConverter<VisitStatus>();

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(40).IsRequired();
            entity.HasData(new Employee
            {
                EmployeeId = 1,
                Username = "admin",
                PasswordHash = PasswordHashService.HashStatic("Admin123!"),
                FullName = "System Administrator",
                Role = "Admin",
                CreatedAt = new DateTime(2026, 1, 1),
                IsActive = true
            });
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(v => v.VisitorId);
            entity.HasIndex(v => v.IdentityNumber).IsUnique();
            entity.Property(v => v.IdentityNumber).HasMaxLength(11).IsRequired();
            entity.Property(v => v.FullName).HasMaxLength(140).IsRequired();
            entity.Property(v => v.PhotoPath).HasMaxLength(500);
        });

        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(r => r.ResidentId);
            entity.HasIndex(r => r.IdentityNumber).IsUnique();
            entity.Property(r => r.IdentityNumber).HasMaxLength(11).IsRequired();
            entity.Property(r => r.FullName).HasMaxLength(140).IsRequired();
            entity.Property(r => r.RoomNumber).HasMaxLength(40);
            entity.Property(r => r.PhotoPath).HasMaxLength(500);
            entity.HasData(
                new Resident { ResidentId = 1, FullName = "Ayse Demir", IdentityNumber = "10000000146", RoomNumber = "A-101", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Resident { ResidentId = 2, FullName = "Mehmet Yilmaz", IdentityNumber = "10000000214", RoomNumber = "B-204", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
                new Resident { ResidentId = 3, FullName = "Elif Kaya", IdentityNumber = "10000000382", RoomNumber = "C-310", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) });
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(v => v.VisitId);
            entity.Property(v => v.VisitReason).HasMaxLength(300).IsRequired();
            entity.Property(v => v.Status).HasConversion(statusConverter).HasMaxLength(20);
            entity.HasOne(v => v.Visitor).WithMany(v => v.Visits).HasForeignKey(v => v.VisitorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(v => v.Resident).WithMany(r => r.Visits).HasForeignKey(v => v.ResidentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(v => new { v.VisitorId, v.Status })
                .HasFilter("[Status] = 'Active'")
                .IsUnique();
        });

        modelBuilder.Entity<BlacklistEntry>(entity =>
        {
            entity.HasKey(b => b.BlacklistEntryId);
            entity.Property(b => b.Reason).HasMaxLength(300).IsRequired();
            entity.HasOne(b => b.Visitor).WithMany().HasForeignKey(b => b.VisitorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(b => new { b.VisitorId, b.IsActive });
        });
    }
}
