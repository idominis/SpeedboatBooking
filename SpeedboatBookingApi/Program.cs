using SpeedboatBookingApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure GoogleSheetsService from configuration
var spreadsheetId = builder.Configuration["GoogleSheets:SpreadsheetId"];
var jsonPath = builder.Configuration["GoogleSheets:CredentialsPath"];

if (string.IsNullOrEmpty(spreadsheetId) || string.IsNullOrEmpty(jsonPath))
{
    throw new InvalidOperationException(
        "Google Sheets configuration is missing. " +
        "Please set GoogleSheets:SpreadsheetId and GoogleSheets:CredentialsPath in user secrets or environment variables.");
}

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
