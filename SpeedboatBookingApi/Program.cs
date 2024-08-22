using SpeedboatBookingApi.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Build configuration
var configuration = builder.Configuration;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

try
{
    Log.Information("Starting web host");

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure GoogleSheetsService
    var spreadsheetId = "1AjyJFcXeGAzWPoF2zYbJuGe2RdmvqXMFa3_fvYTUwA0";
    var jsonPath = "C:\\Users\\ido\\OneDrive\\SpeedboatBookingApp\\speedboatbookingapp-28f41b29a0c0.json"; // Update this path if needed
    builder.Services.AddSingleton(new GoogleSheetsService(spreadsheetId, jsonPath));

    // Configure CORS to allow requests from the Blazor app
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorApp",
            policy => policy.WithOrigins("https://localhost:7293")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials());
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging(); // Add Serilog request logging

    app.UseHttpsRedirection();

    app.UseCors("AllowBlazorApp"); // Apply the CORS policy

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
