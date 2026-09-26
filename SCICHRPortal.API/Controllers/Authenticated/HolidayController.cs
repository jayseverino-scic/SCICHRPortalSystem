using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing.Printing;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private IHolidayService HolidayService { get; }
        public HolidayController(IHolidayService holidayService)
        {
            HolidayService = holidayService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var holiday = await HolidayService.GetAllAsync();
            return Ok(holiday.Select(d => ToRow(d, 0)));
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await HolidayService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var data = tuple.Item1.Select(d => ToRow(d, orderNumber++));

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };
            return Ok(dto);
        }

        private static object ToRow(Holiday d, int orderNumber)
        {
            var projects = d.Projects.OrderBy(project => project.Id)
                .Select(project => new {
                    project.Id,
                    Name = project.Name ?? $"Project #{project.Id}",
                    project.Deleted
                }).ToArray();
            return new
            {
                d.HolidayId,
                d.HolidayName,
                d.HolidayType,
                HolidayTypeName = d.HolidayType switch
                {
                    (int)HolidayType.Regular => "Regular",
                    (int)HolidayType.SpecialNonWorking => "Special Non-Working",
                    (int)HolidayType.Local => "Local",
                    _ => "Unknown"
                },
                d.HolidayDate,
                ProjectName = projects.Length == 0 ? null : string.Join(", ", projects.Select(project => project.Name)),
                AllProjects = projects.Length == 0,
                ProjectIds = projects.Select(project => project.Id).ToArray(),
                ProjectNames = projects.Select(project => project.Name).ToArray(),
                Projects = projects,
                IsTodayAnnouncement = DateTime.Today == d.CreatedAt.Date,
                d.CreatedAt,
                OrderNumber = orderNumber
            };
        }


        [HttpPost()]
        public async Task<IActionResult> InsertAsync(HolidayCreateRequest holiday)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await HolidayService.CreateAsync(holiday);
            return result.Status switch
            {
                HolidayWriteStatus.Invalid => BadRequest(new { result.Message }),
                HolidayWriteStatus.Conflict => Conflict(new { result.Message }),
                _ => StatusCode(201)
            };
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(HolidayUpdateRequest holiday)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await HolidayService.UpdateAsync(holiday);
            return result.Status switch
            {
                HolidayWriteStatus.Invalid => BadRequest(new { result.Message }),
                HolidayWriteStatus.Conflict => Conflict(new { result.Message }),
                HolidayWriteStatus.NotFound => NotFound(ResponseMessage.NotFound),
                _ => Ok()
            };
        }


        [HttpDelete("{holidayId}")]
        public async Task<IActionResult> DeleteAsync(int holidayId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await HolidayService.DeleteAsync(holidayId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
