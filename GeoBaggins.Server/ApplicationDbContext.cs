using GeoBaggins.AdminApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoBaggins.Server;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<AnchorZone> GeoZones { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}