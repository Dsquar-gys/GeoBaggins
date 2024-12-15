using GeoBaggins.AdminApp.Models;
using GeoBaggins.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoBaggins.AdminApp.Data;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<AnchorZone> GeoZones { get; set; }
    
    public DbSet<DeviceState> DeviceStates { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnchorZone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Radius).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500);
        });

        modelBuilder.Entity<DeviceState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.LastNotificationTime).HasColumnType("datetime");
            entity.HasOne<AnchorZone>()
                .WithMany()
                .HasForeignKey(e => e.GeoZoneId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }
}