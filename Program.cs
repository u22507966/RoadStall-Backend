using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using RoadStallAPI;
using RoadStallAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure database based on environment
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<RoadStallDbContext>(options =>
    options.UseSqlServer(connectionString));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Added cors because it needs to allow angular to receive calls
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "https://roadstallprototype.netlify.app",
                    "https://roadstallprototype2.netlify.app",
                    "https://famous-conkies-e08abb.netlify.app"
                )   
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoadStallDbContext>();
    
    try
    {
        Console.WriteLine("Applying database migrations...");
        db.Database.Migrate();                                                                  //----------------------comment out if crashing upon migration------------------------
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        // DON'T throw - let app start even if migrations fail (for debugging)
        // throw; // Commented out temporarily to allow app to start
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Only use HTTPS redirection in production to avoid development issues
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "RoadStall API is running");

app.Run();
