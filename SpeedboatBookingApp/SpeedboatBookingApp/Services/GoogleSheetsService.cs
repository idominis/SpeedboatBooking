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
}
