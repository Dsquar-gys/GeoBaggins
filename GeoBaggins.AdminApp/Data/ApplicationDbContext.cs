using GeoBaggins.AdminApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoBaggins.AdminApp.Data;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<AnchorZone> GeoZones { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}