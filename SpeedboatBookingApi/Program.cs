using SpeedboatBookingApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure GoogleSheetsService
var spreadsheetId = "1AjyJFcXeGAzWPoF2zYbJuGe2RdmvqXMFa3_fvYTUwA0";
var jsonPath = "C:\\Users\\ido\\OneDrive\\SpeedboatBookingApp\\speedboatbookingapp-28f41b29a0c0.json"; // Update this path if needed
builder.Services.AddSingleton(new GoogleSheetsService(spreadsheetId, jsonPath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
