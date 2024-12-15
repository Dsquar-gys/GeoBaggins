using GeoBaggins.Models;
using GeoBaggins.Server;
using Microsoft.AspNetCore.Http.HttpResults;
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
            // Чтение тела запроса
            var locationData = await context.Request.ReadFromJsonAsync<LocationDto>();
            
            if (locationData == null)
            {
                return Results.BadRequest("Invalid data");
            }

            // Логика обработки данных
            Console.WriteLine($"Received location: Lat={locationData.Latitude}, Lng={locationData.Longitude}, Timestamp={locationData.TimeStamp}");

            var responseMessage = @"{Сообщение от кофейни}";
            
            // Здесь можно сохранить данные в базу данных или выполнить другую обработку
            return Results.Ok(responseMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Results.StatusCode(500);
        }
    })
    .WithName("LocationCheck")
    .WithOpenApi();

app.MapGet("/db", (ApplicationDbContext dbContext) =>
{
    var areas = dbContext.GeoZones.ToList();
    return Results.Ok(areas);
});

app.Run();