using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing.Printing;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private IProjectService ProjectService { get; }
        private XCompanyBranchService CompanyBranchService { get; }
        public ProjectController(IProjectService projectService, XCompanyBranchService companyBranchService)
        {
            ProjectService = projectService;
            CompanyBranchService = companyBranchService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var project = await ProjectService.GetAllAsync();
            return Ok(project);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await ProjectService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.Code,
                d.Name,
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
        public async Task<IActionResult> InsertAsync(Project project)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await ProjectService.HasDuplicateName(project);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            project.CreatedAt = DateTime.Now;
            project.CreatedBy = "manuel";
            await ProjectService.InsertAsync(project);

            return StatusCode(201, project.Id);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Project project)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            project.UpdatedBy = "manuel";
            project.UpdatedAt = DateTime.Now;
            var updated = await ProjectService.UpdateAsync(project);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteAsync(int projectId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await ProjectService.DeleteAsync(projectId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb()
        {
            var xProjects = await CompanyBranchService.GetAllAsync();
            var existingProjects = await ProjectService.GetAllAsync();
            var projectList = new List<Project>();
            if (xProjects != null)
            {
                foreach (var project in xProjects)
                {
                    Project projectQuery = existingProjects.Where(d => d.Name?.ToUpper() == project.Name?.ToUpper()).SingleOrDefault();
                    if (projectQuery == null)
                    {
                        Project newProject = new()
                        {
                            Code = project.Code,
                            Name = project.Name,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "manuel"

                        };
                        projectList.Add(newProject);
                        await ProjectService.InsertAsync(newProject);
                    }
                }
            }
            var dto = new
            {
                Data = projectList,
                Total = projectList.Count()
            };
            return Ok(dto);
        }
    }
}
