using GeoBaggins.Models;
using GeoBaggins.Server;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/location", async (HttpContext context) =>
    {
        try
        {
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            var locationData = await context.Request.ReadFromJsonAsync<LocationDto>();
            
            if (locationData == null)
            {
                return Results.BadRequest("Invalid data");
            }
            
            var zones = await dbContext.GeoZones.ToListAsync();
            var lastZone = zones.FirstOrDefault(x =>
                Extends.IsWithinGeoZone(locationData.Latitude, locationData.Longitude, x));
            
            if (lastZone == null)
            {
                return Results.StatusCode(StatusCodes.Status100Continue);
            }
            
            var deviceState = await dbContext.DeviceStates
                .FirstOrDefaultAsync(ds => ds.DeviceId == locationData.DeviceId);
            
            if (deviceState == null)
            {
                deviceState = new DeviceState
                {
                    DeviceId = locationData.DeviceId,
                    GeoZoneId = null,
                    LastNotificationTime = DateTime.UtcNow
                };

                dbContext.DeviceStates.Add(deviceState);
                dbContext.SaveChanges();
                
                return Results.StatusCode(StatusCodes.Status100Continue);
            }

            var isNewZone = deviceState.GeoZoneId != lastZone.Id;
            var isTimeout = deviceState.LastNotificationTime == null || 
                            (DateTime.UtcNow - deviceState.LastNotificationTime.Value).TotalHours >= 3;

            if (!isNewZone && !isTimeout) return Results.StatusCode(StatusCodes.Status100Continue);
            
            deviceState.GeoZoneId = lastZone.Id;
            deviceState.LastNotificationTime = DateTime.UtcNow;
            
            dbContext.SaveChanges();

            return Results.Ok(lastZone.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.StatusCode(500);
        }
    })
    .WithName("LocationCheck")
    .WithOpenApi();

app.Run();