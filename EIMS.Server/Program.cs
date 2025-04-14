using Microsoft.EntityFrameworkCore;
using EIMS.Server.Data;
using EIMS.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register application services
builder.Services.AddScoped<ILocationGenerationService, LocationGenerationService>();

// Configure CORS to allow requests from the client
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientAppPolicy", policy =>
    {
        // For production, replace this with specific origins, methods, and headers
        // For example: policy.WithOrigins("https://your-production-domain.com")
        //              .WithMethods("GET", "POST", "PUT", "DELETE")
        //              .WithHeaders("Content-Type", "Authorization");
        
        // Very permissive CORS for development - DO NOT use in production
        policy.WithOrigins("http://localhost:5198")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Add this to allow credentials
              
        // Also consider setting up a reverse proxy in production to avoid CORS issues entirely
    });
});

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

// Use CORS - important to place this before other middleware
app.UseCors("ClientAppPolicy");

// Disable HTTPS redirection for development
// app.UseHttpsRedirection();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
