using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private IRoleService RoleService { get; }
        public RoleController(IRoleService roleService)
        {
            RoleService = roleService;
        }

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var roles = await RoleService.GetAllAsync();

            return Ok(roles);
        }

        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await RoleService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.RoleId,
                d.Description,
                d.Name,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> InsertAsync(Role role)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await RoleService.HasDuplicateName(role);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            await RoleService.InsertAsync(role);

            return StatusCode(201, role.RoleId);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Role role)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var updated = await RoleService.UpdateAsync(role);
            if(!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        [Authorize]
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteAsync(int roleId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await RoleService.DeleteAsync(roleId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
