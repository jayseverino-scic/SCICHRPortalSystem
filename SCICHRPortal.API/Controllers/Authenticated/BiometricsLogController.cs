using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class BiometricsLogController : ControllerBase
    {
        private IBiometricsLogService BiometricsLogService { get; }

        public BiometricsLogController(IBiometricsLogService biometricsLogService)
        {
            BiometricsLogService = biometricsLogService;

        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var biometricsLogs = await BiometricsLogService.GetAllAsync();
            return Ok(biometricsLogs);
        }
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate)
        {
            var tuple = await BiometricsLogService.FilterAsync(pageNumber, pageSize, searchKeyword!, startDate, endDate);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.BiometricsLogId,
                d.TMNo,
                d.EmployeeNo,
                d.EmployeeName,
                d.GMNo,
                d.Mode,
                d.InOut,
                d.AntiPass,
                d.ProxyWork,
                d.DateTimeLog,
                d.CreatedAt,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };
            return Ok(dto);
        }

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(BiometricsLog biometricsLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            await BiometricsLogService.InsertAsync(biometricsLog);

            return StatusCode(201, biometricsLog.BiometricsLogId);
        }
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(BiometricsLog biometricsLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var updated = await BiometricsLogService.UpdateAsync(biometricsLog);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
        [HttpPost("Import")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName);
            if (extension != ".xlsx")
            {
                return StatusCode(415, ResponseMessage.FileNotSupported);
            }
            var biometricsLogs = new List<BiometricsLog>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                var rowCount = workSheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var tmNo = workSheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                        var employeeNo = workSheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
                        var employeeName = workSheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                        var gmNo = workSheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
                        var mode = workSheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                        var inOut = workSheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "";
                        var antiPass = workSheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "";
                        var proxyWork = workSheet.Cells[row, 8].Value?.ToString()?.Trim() ?? "";
                        var dateTimeLog = workSheet.Cells[row, 9].Value?.ToString()?.Trim() ?? "";

                        //if (string.IsNullOrWhiteSpace(employeeNo) || string.IsNullOrWhiteSpace(employeeName) || string.IsNullOrWhiteSpace(inOut)
                        //    || string.IsNullOrWhiteSpace(dateTimeLog))
                        //    return StatusCode(422, $"One or more fields invalid at row {row}");

                        BiometricsLog biometricsLog = new()
                        {
                            TMNo = Convert.ToInt32(tmNo),
                            EmployeeNo = employeeNo,
                            EmployeeName = employeeName,
                            GMNo = Convert.ToInt32(gmNo),
                            Mode = mode,
                            InOut = inOut,
                            AntiPass = Convert.ToInt32(antiPass),
                            ProxyWork = Convert.ToInt32(proxyWork),
                            DateTimeLog = Convert.ToDateTime(dateTimeLog)
                        };
                        biometricsLogs.Add(biometricsLog);
                        await BiometricsLogService.InsertAsync(biometricsLog);
                    }
                    catch (Exception)
                    {
                        return StatusCode(422, $"One or more fields invalid at row {row}");
                    }
                }
            }
            var dto = new
            {
                Data = biometricsLogs,
                Total = biometricsLogs.Count()
            };
            return Ok(dto);
        }
    }
}
