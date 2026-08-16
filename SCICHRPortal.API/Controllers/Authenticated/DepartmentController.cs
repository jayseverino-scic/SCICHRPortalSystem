using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;
using System.Drawing.Printing;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private IDepartmentService DepartmentService { get; }
        private IXDepartmentService XDepartmentService { get; }
        public DepartmentController(IDepartmentService departmentService, IXDepartmentService xDepartmentService)
        {
            DepartmentService = departmentService;
            XDepartmentService = xDepartmentService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var department = await DepartmentService.GetAllAsync();
            return Ok(department);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await DepartmentService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.DepartmentId,
                d.DeptCode,
                d.DepartmentName,
                IsTodayAnnouncement = dateToday.Date == d.CreatedAt.Date,
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
        public async Task<IActionResult> InsertAsync(Department department)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await DepartmentService.HasDuplicateName(department);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            department.CreatedAt = DateTime.Now;
            department.CreatedBy = "manuel";
            await DepartmentService.InsertAsync(department);

            return StatusCode(201, department.DepartmentId);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Department department)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            department.UpdatedBy = "manuel";
            department.UpdatedAt = DateTime.Now;
            var updated = await DepartmentService.UpdateAsync(department);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{departmentId}")]
        public async Task<IActionResult> DeleteAsync(int departmentId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await DepartmentService.DeleteAsync(departmentId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb()
        {
            var xDepartments = await XDepartmentService.GetAllAsync();
            var existingDepartments = await DepartmentService.GetAllAsync();
            var deparmentList = new List<Department>();
            if (xDepartments != null)
            {
                foreach (var department in xDepartments)
                {
                    Department departmentQuery = existingDepartments.Where(d => d.DepartmentName?.ToUpper() == department.Name?.ToUpper()).SingleOrDefault();
                    if (departmentQuery == null)
                    {
                        Department newDepartment = new()
                        {
                            DeptCode = department.Id.ToString(),
                            DepartmentName = department.Name,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "manuel"

                        };
                        deparmentList.Add(newDepartment);
                        await DepartmentService.InsertAsync(newDepartment);
                    }
                }
            }
            var dto = new
            {
                Data = deparmentList,
                Total = deparmentList.Count()
            };
            return Ok(dto);
        }
    }
}
