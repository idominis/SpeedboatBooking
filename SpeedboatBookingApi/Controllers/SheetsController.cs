﻿using Microsoft.AspNetCore.Mvc;
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
    }
}
