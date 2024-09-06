using SpeedboatBookingApp.Models;
using System.Net.Http.Json;

public class GoogleSheetsService
{
    private readonly HttpClient _httpClient;

    public GoogleSheetsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<string>> GetSpeedboatsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7089/api/sheets/GetSpeedboatNames");
        return result ?? new List<string>();
    }

    public async Task<List<string>> GetBookersAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7089/api/sheets/GetBookerNames");
        return result ?? new List<string>();
    }

    public async Task<int?> GetColumnIndexBySpeedboatNameAsync(string sheetName, string speedboatName)
    {
        var result = await _httpClient.GetFromJsonAsync<int?>($"https://localhost:7089/api/sheets/getColumnIndexBySpeedboatName?sheetName={sheetName}&speedboatName={speedboatName}");
        return result;
    }

    public async Task<int?> GetRowIndexByDateAsync(string sheetName, DateTime date)
    {
        var result = await _httpClient.GetFromJsonAsync<int?>($"https://localhost:7089/api/sheets/getRowIndexByDate?sheetName={sheetName}&date={date.ToString("yyyy-MM-dd")}");
        return result;
    }

    // Method to check if a cell is bookable
    public async Task<bool> IsCellBookableAsync(string sheetName, int rowIndex, int columnIndex)
    {
        var result = await _httpClient.GetFromJsonAsync<BookableResponse>($"https://localhost:7089/api/sheets/isCellBookable?sheetName={sheetName}&rowIndex={rowIndex}&columnIndex={columnIndex}");
        return result.Bookable;
    }

    // Method to update the cell color
    public async Task UpdateCellColorAsync(string sheetName, int rowIndex, int columnIndex, float red, float green, float blue)
    {
        var colorParams = new { red, green, blue };
        var url = $"https://localhost:7089/api/sheets/updateCellColor?sheetName={sheetName}&rowIndex={rowIndex}&columnIndex={columnIndex}&red={red}&green={green}&blue={blue}";
        await _httpClient.PostAsync(url, null);
    }

    // Method to update the text color
    public async Task UpdateTextColorAsync(string sheetName, int rowIndex, int columnIndex, float red, float green, float blue)
    {
        var url = $"https://localhost:7089/api/sheets/updateTextColor?sheetName={sheetName}&rowIndex={rowIndex}&columnIndex={columnIndex}&red={red}&green={green}&blue={blue}";
        await _httpClient.PostAsync(url, null);
    }

    // Method to enter the renter's name in the cell
    public async Task EnterRenterNameAsync(string sheetName, DateTime date, string speedboatName, string renterName)
    {
        var url = $"https://localhost:7089/api/sheets/enterRenterName?sheetName={sheetName}&date={date:yyyy-MM-dd}&speedboatName={speedboatName}&renterName={renterName}";
        await _httpClient.PostAsync(url, null);
    }

    // Method to get sheet data for display in a table
    public async Task<IList<IList<object>>> GetSheetDataAsync(string sheetName)
    {
        var result = await _httpClient.GetFromJsonAsync<IList<IList<object>>>($"https://localhost:7089/api/sheets/{sheetName}");
        return result ?? new List<IList<object>>();
    }

    // Method to get sheet data for display in a table with cell colors
    public async Task<IList<IList<CellDataResponse>>> GetSheetDataWithColorsAsync(string sheetName)
    {
        var result = await _httpClient.GetFromJsonAsync<IList<IList<CellDataResponse>>>($"https://localhost:7089/api/sheets/getDataWithColors?sheetName={sheetName}");
        return result ?? new List<IList<CellDataResponse>>();
    }
}
