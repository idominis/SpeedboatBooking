using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SpeedboatBookingApi.Services
{
    public class GoogleSheetsService
    {
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly SheetsService _sheetsService;
        private readonly string _spreadsheetId;

        public GoogleSheetsService(string spreadsheetId, string jsonPath)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

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

        public async Task UpdateCellColorAsync(string sheetName, int rowIndex, int columnIndex, float red, float green, float blue)
        {
            var request = new Request
            {
                RepeatCell = new RepeatCellRequest
                {
                    Range = new GridRange
                    {
                        SheetId = await GetSheetIdAsync(sheetName),
                        StartRowIndex = rowIndex,
                        EndRowIndex = rowIndex + 1,
                        StartColumnIndex = columnIndex,
                        EndColumnIndex = columnIndex + 1
                    },
                    Cell = new CellData
                    {
                        UserEnteredFormat = new CellFormat
                        {
                            BackgroundColor = new Color
                            {
                                Red = red,
                                Green = green,
                                Blue = blue
                            }
                        }
                    },
                    Fields = "userEnteredFormat.backgroundColor"
                }
            };

            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request> { request }
            };

            var batchUpdate = _sheetsService.Spreadsheets.BatchUpdate(batchUpdateRequest, _spreadsheetId);
            await batchUpdate.ExecuteAsync();
            }

        private async Task<int> GetSheetIdAsync(string sheetName)
        {
            var spreadsheet = await _sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);

            if (sheet == null)
                throw new Exception($"Sheet with name '{sheetName}' not found.");

            return (int)sheet.Properties.SheetId;
        }

        // Method to get the row index by date
        public async Task<int?> GetRowIndexByDateAsync(string sheetName, DateTime date)
        {
            var range = $"{sheetName}!A:A";
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
            var response = await request.ExecuteAsync();

            if (response.Values != null)
            {
                for (int i = 0; i < response.Values.Count; i++)
                {
                    if (DateTime.TryParse(response.Values[i][0]?.ToString(), out DateTime rowDate) && rowDate == date)
                    {
                        return i;
                    }
                }
            }

            return null; // Return null if the date is not found
        }

        // Method to get the column index by speedboat name
        public async Task<int?> GetColumnIndexBySpeedboatNameAsync(string sheetName, string speedboatName)
        {
            var range = $"{sheetName}!2:2";
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
            var response = await request.ExecuteAsync();

            if (response.Values != null && response.Values.Count > 0)
            {
                var headerRow = response.Values[0];
                for (int i = 0; i < headerRow.Count; i++)
                {
                    if (headerRow[i]?.ToString().Equals(speedboatName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        return i;
                    }
                }
            }

            return null; // Return null if the speedboat name is not found
        }

        // Method to enter renter's name in the corresponding cell
        public async Task EnterRenterNameAsync(string sheetName, int rowIndex, int columnIndex, string renterName)
        {
            var range = $"{sheetName}!{(char)('A' + columnIndex)}{rowIndex + 1}";
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { new List<object> { renterName } }
            };

            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await updateRequest.ExecuteAsync();
        }


    }
}
