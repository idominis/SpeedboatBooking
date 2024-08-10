using Microsoft.AspNetCore.Mvc;
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
    }
}
