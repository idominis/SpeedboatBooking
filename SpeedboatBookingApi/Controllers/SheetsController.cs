using Microsoft.AspNetCore.Mvc;
using Serilog;
using SpeedboatBookingApi.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedboatBookingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SheetsController : ControllerBase
    {
        private readonly GoogleSheetsService _sheetsService;

        public SheetsController(GoogleSheetsService sheetsService)
        {
            _sheetsService = sheetsService;
        }

        [HttpGet("{sheetName}")]
        public async Task<IActionResult> GetSheetData(string sheetName)
        {
            var data = await _sheetsService.GetSheetDataAsync(sheetName);
            return Ok(data);
        }

        [HttpGet("getDataWithColors")]
        public async Task<IActionResult> GetDataWithColors(string sheetName)
        {
            try
            {
                var data = await _sheetsService.GetSheetDataWithColorsAsync(sheetName);
                return Ok(data);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while getting sheet data with colors.");
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpPost("updateCellColor")]
        public async Task<IActionResult> UpdateCellColor(string sheetName, int rowIndex, int columnIndex, float red, float green, float blue)
        {
            try
            {
                await _sheetsService.UpdateCellColorAsync(sheetName, rowIndex, columnIndex, red, green, blue); // 1,0,0 red color
                return Ok(new { message = "Cell color updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("updateTextColor")]
        public async Task<IActionResult> UpdateTextColor(string sheetName, int rowIndex, int columnIndex, float red, float green, float blue)
        {
            try
            {
                await _sheetsService.UpdateTextColorAsync(sheetName, rowIndex, columnIndex, red, green, blue);
                return Ok(new { message = "Text color updated successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to update the text color.");
                return BadRequest(new { message = ex.Message });
            }
        }

        // API to get the row index by date
        [HttpGet("getRowIndexByDate")]
        public async Task<IActionResult> GetRowIndexByDate(string sheetName, DateTime date)
        {
            var rowIndex = await _sheetsService.GetRowIndexByDateAsync(sheetName, date);
            if (rowIndex.HasValue)
            {
                return Ok(rowIndex.Value);
            }
            return NotFound(new { message = "Date not found" });
        }

        // API to get the column index by speedboat name
        [HttpGet("getColumnIndexBySpeedboatName")]
        public async Task<IActionResult> GetColumnIndexBySpeedboatName(string sheetName, string speedboatName)
        {
            var columnIndex = await _sheetsService.GetColumnIndexBySpeedboatNameAsync(sheetName, speedboatName);
            if (columnIndex.HasValue)
            {
                return Ok(columnIndex.Value);
            }
            return NotFound(new { message = "Speedboat name not found" });
        }

        // API to enter renter's name in the corresponding cell
        [HttpPost("enterRenterName")]
        public async Task<IActionResult> EnterRenterName(string sheetName, DateTime date, string speedboatName, string renterName)
        {
            var rowIndex = await _sheetsService.GetRowIndexByDateAsync(sheetName, date);
            var columnIndex = await _sheetsService.GetColumnIndexBySpeedboatNameAsync(sheetName, speedboatName);

            if (rowIndex.HasValue && columnIndex.HasValue)
            {
                await _sheetsService.EnterRenterNameAsync(sheetName, rowIndex.Value, columnIndex.Value, renterName);
                return Ok(new { message = "Renter's name entered successfully" });
            }
            return BadRequest(new { message = "Could not find the specified date or speedboat name" });
        }

        [HttpGet("getCellValue")]
        public async Task<IActionResult> GetCellValue(string sheetName, int rowIndex, int columnIndex)
        {
            try
            {
                var cellValue = await _sheetsService.GetCellValueAsync(sheetName, rowIndex, columnIndex);
                if (cellValue != null)
                {
                    return Ok(new { value = cellValue });
                }
                return NotFound(new { message = "Cell is empty or not found." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getCellBackgroundColor")]
        public async Task<IActionResult> GetCellBackgroundColor(string sheetName, int rowIndex, int columnIndex)
        {
            try
            {
                var cellColor = await _sheetsService.GetCellBackgroundColorAsync(sheetName, rowIndex, columnIndex);
                if (cellColor != null)
                {
                    return Ok(new
                    {
                        red = cellColor.Red,
                        green = cellColor.Green,
                        blue = cellColor.Blue
                    });
                }
                return Ok(new { message = "Cell has no explicit background color set; default is being used." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to retrieve the cell background color.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getSpeedboatNames")]
        public async Task<IActionResult> GetSpeedboatNames()
        {
            try
            {
                var speedboatNames = await _sheetsService.GetSpeedboatNamesAsync();
                if (speedboatNames != null && speedboatNames.Count > 0)
                {
                    return Ok(speedboatNames);
                }
                return NotFound(new { message = "No speedboat names found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to retrieve speedboat names.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getBookerNames")]
        public async Task<IActionResult> GetBookerNames()
        {
            try
            {
                var bookerNames = await _sheetsService.GetBookerNamesAsync();
                if (bookerNames != null && bookerNames.Count > 0)
                {
                    return Ok(bookerNames);
                }
                return NotFound(new { message = "No booker names found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to retrieve booker names.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("isCellBookable")]
        public async Task<IActionResult> IsCellBookable(string sheetName, int rowIndex, int columnIndex)
        {
            try
            {
                var isBookable = await _sheetsService.IsCellBookableAsync(sheetName, rowIndex, columnIndex);
                return Ok(new { bookable = isBookable });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to check if the cell is bookable.");
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
