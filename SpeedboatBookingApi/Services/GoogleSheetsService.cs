using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using Serilog;
using SpeedboatBookingApi.Models;

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

        public async Task<List<List<CellDataResponse>>> GetSheetDataWithColorsAsync(string sheetName)
        {
            var sheet = await _sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
            var sheetId = sheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName)?.Properties.SheetId;

            if (sheetId == null)
            {
                throw new Exception($"Sheet with name '{sheetName}' not found.");
            }

            var request = _sheetsService.Spreadsheets.Get(_spreadsheetId);
            request.Ranges = new List<string> { $"{sheetName}!A1:Z" };
            request.Fields = "sheets.data.rowData.values.userEnteredFormat,sheets.data.rowData.values.effectiveValue";

            var response = await request.ExecuteAsync();
            var sheetData = response.Sheets.FirstOrDefault()?.Data.FirstOrDefault();

            var result = new List<List<CellDataResponse>>();

            if (sheetData?.RowData != null)
            {
                foreach (var rowData in sheetData.RowData)
                {
                    var row = new List<CellDataResponse>();
                    foreach (var cell in rowData.Values ?? Enumerable.Empty<CellData>())
                    {
                        string backgroundColor = null;
                        string textColor = null;
                        string cellValue = null;

                        if (cell.UserEnteredFormat?.BackgroundColor != null)
                        {
                            var color = cell.UserEnteredFormat.BackgroundColor;
                            backgroundColor = $"#{(int)((color.Red ?? 0) * 255):X2}{(int)((color.Green ?? 0) * 255):X2}{(int)((color.Blue ?? 0) * 255):X2}";
                        }

                        if (cell.UserEnteredFormat?.TextFormat?.ForegroundColor != null)
                        {
                            var color = cell.UserEnteredFormat.TextFormat.ForegroundColor;
                            textColor = $"#{(int)((color.Red ?? 0) * 255):X2}{(int)((color.Green ?? 0) * 255):X2}{(int)((color.Blue ?? 0) * 255):X2}";
                        }

                        // If the effective value is a number, try to determine if it's a date
                        if (cell.EffectiveValue?.NumberValue.HasValue == true)
                        {
                            // Check if the number format is a date format
                            if (cell.UserEnteredFormat?.NumberFormat?.Type == "DATE" ||
                                cell.UserEnteredFormat?.NumberFormat?.Type == "DATE_TIME")
                            {
                                var numberValue = cell.EffectiveValue.NumberValue.Value;
                                DateTime dateValue = DateTime.FromOADate(numberValue);  // Convert the number to a date
                                cellValue = dateValue.ToString("d.M.yyyy");  // Adjust the format as per your needs
                            }
                            else
                            {
                                // If it's not a date, treat it as a regular number
                                cellValue = cell.EffectiveValue.NumberValue?.ToString();
                            }
                        }
                        else
                        {
                            // Use the string or boolean representation if it's not a number
                            cellValue = cell.EffectiveValue?.StringValue ?? cell.EffectiveValue?.BoolValue?.ToString();
                        }

                        row.Add(new CellDataResponse
                        {
                            Value = cellValue,
                            BackgroundColor = backgroundColor,
                            TextColor = textColor
                        });
                    }
                    result.Add(row);
                }
            }

            return result;
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

        public async Task UpdateTextColorAsync(string sheetName, int rowIndex, int columnIndex, float red, float green, float blue)
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
                            TextFormat = new TextFormat
                            {
                                ForegroundColor = new Color
                                {
                                    Red = red,
                                    Green = green,
                                    Blue = blue
                                }
                            }
                        }
                    },
                    Fields = "userEnteredFormat.textFormat.foregroundColor"
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
                    // Check if the current row has any values
                    if (response.Values[i].Count > 0)
                    {
                        string cellValue = response.Values[i][0]?.ToString();

                        // Attempt to parse the date using the expected format "d.M."
                        if (DateTime.TryParseExact(cellValue,
                                                   "d.M.",
                                                   CultureInfo.InvariantCulture,
                                                   DateTimeStyles.None,
                                                   out DateTime rowDate))
                        {
                            // Compare only day and month
                            if (rowDate.Day == date.Day && rowDate.Month == date.Month)
                            {
                                return i; // Return the row index if the date matches
                            }
                        }
                    }
                }
            }

            return null; // Return null if the date is not found
        }

        // Method to get the column index by speedboat name
        public async Task<int?> GetColumnIndexBySpeedboatNameAsync(string sheetName, string speedboatName)
        {
            var range = $"{sheetName}!1:1";
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

        // Method to read the value from a specific cell
        public async Task<string> GetCellValueAsync(string sheetName, int rowIndex, int columnIndex)
        {
            var range = $"{sheetName}!{(char)('A' + columnIndex)}{rowIndex + 1}";
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
            var response = await request.ExecuteAsync();

            if (response.Values != null && response.Values.Count > 0 && response.Values[0].Count > 0)
            {
                return response.Values[0][0]?.ToString();
            }

            return null; // Return null if the cell is empty or not found
        }

        public async Task<Color?> GetCellBackgroundColorAsync(string sheetName, int rowIndex, int columnIndex)
        {
            Log.Information("Attempting to retrieve full cell data for {SheetName}, row {RowIndex}, column {ColumnIndex}",
                sheetName, rowIndex, columnIndex);

            var sheet = await _sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
            var sheetId = sheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName)?.Properties.SheetId;

            if (sheetId == null)
            {
                Log.Warning("Sheet with name '{SheetName}' not found", sheetName);
                throw new Exception($"Sheet with name '{sheetName}' not found.");
            }

            var request = _sheetsService.Spreadsheets.Get(_spreadsheetId);
            request.Ranges = new List<string> { $"{sheetName}!{(char)('A' + columnIndex)}{rowIndex + 1}" };
            request.Fields = "sheets.data.rowData.values.userEnteredFormat"; // Request the full user-entered format

            var response = await request.ExecuteAsync();
            var sheetData = response.Sheets.FirstOrDefault()?.Data.FirstOrDefault();

            if (sheetData?.RowData != null && sheetData.RowData.Count > 0)
            {
                var cell = sheetData.RowData[0].Values?[0];
                var userEnteredFormat = cell?.UserEnteredFormat;

                if (userEnteredFormat != null)
                {
                    Log.Information("Retrieved full cell format: {Format}", userEnteredFormat);
                    Log.Information("Background Color: {BackgroundColor}", userEnteredFormat.BackgroundColor);
                    Log.Information("Text Format: {TextFormat}", userEnteredFormat.TextFormat);
                    Log.Information("Borders: {Borders}", userEnteredFormat.Borders);
                    Log.Information("Horizontal Alignment: {HorizontalAlignment}", userEnteredFormat.HorizontalAlignment);

                    return userEnteredFormat.BackgroundColor; // Return the background color
                }
                else
                {
                    Log.Warning("No user-entered format found for the specified cell.");
                }
            }
            else
            {
                Log.Warning("No data found for the specified cell.");
            }

            return null; // Return null if no color is found
        }

        public async Task<List<string>> GetSpeedboatNamesAsync()
        {
            var range = "Availability2024!C1:P1"; // Hardcoded range for speedboat names (row 0, columns 2-15)
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
            var response = await request.ExecuteAsync();

            var speedboatNames = new List<string>();
            if (response.Values != null && response.Values.Count > 0)
            {
                var headerRow = response.Values[0];
                speedboatNames.AddRange(headerRow.Select(cell => cell?.ToString()).Where(name => !string.IsNullOrEmpty(name)));
            }

            return speedboatNames;
        }

        public async Task<List<string>> GetBookerNamesAsync()
        {
            var range = "bookers!C2:C"; // Assuming booker names are in column A, starting from row 1
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
            var response = await request.ExecuteAsync();

            var bookerNames = new List<string>();
            if (response.Values != null && response.Values.Count > 0)
            {
                foreach (var row in response.Values)
                {
                    if (row.Count > 0 && !string.IsNullOrEmpty(row[0]?.ToString()))
                    {
                        bookerNames.Add(row[0].ToString());
                    }
                }
            }

            return bookerNames;
        }

        public async Task<bool> IsCellBookableAsync(string sheetName, int rowIndex, int columnIndex)
        {
            var cellColor = await GetCellBackgroundColorAsync(sheetName, rowIndex, columnIndex);

            // Define the green color you consider as "bookable"
            var bookableGreenColor = new Color
            {
                Red = 0.0f,
                Green = 1.0f, // Pure green
                Blue = 0.0f
            };

            if (cellColor != null)
            {
                // Compare the cell color with the green color (tolerance might be needed due to float precision)
                return IsColorEqual(cellColor, bookableGreenColor);
            }

            // If no color is explicitly set, assume it's bookable or not depending on your logic.
            // Returning true by default if there's no color set.
            return true;
        }

        private bool IsColorEqual(Color color1, Color color2)
        {
            // Use ?? to provide a default value of 0.0f in case the color component is null
            return Math.Abs((color1.Red ?? 0.0f) - (color2.Red ?? 0.0f)) < 0.01 &&
                   Math.Abs((color1.Green ?? 0.0f) - (color2.Green ?? 0.0f)) < 0.01 &&
                   Math.Abs((color1.Blue ?? 0.0f) - (color2.Blue ?? 0.0f)) < 0.01;
        }


    }
}
