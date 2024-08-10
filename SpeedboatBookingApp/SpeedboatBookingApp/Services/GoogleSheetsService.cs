using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GoogleSheetsService
{
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private readonly SheetsService _sheetsService;
    private readonly string _spreadsheetId;

    public GoogleSheetsService(string spreadsheetId, string credentialJson)
    {
        var credential = GoogleCredential.FromJson(credentialJson).CreateScoped(Scopes);
        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "SpeedboatBookingApp",
        });
        _spreadsheetId = spreadsheetId;
    }

    public async Task<IList<IList<object>>> GetSheetDataAsync(string sheetName)
    {
        var range = $"{sheetName}!A1:Z";
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values;
    }
}
