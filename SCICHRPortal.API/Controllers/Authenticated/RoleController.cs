using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize(Roles = "Administrator")]
    [SCICHRPortal.API.Authorization.CurrentAdministrator]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private IRoleService RoleService { get; }
        private int ActorId => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.Sid)?.Value, out var id) ? id : 0;
        private string Actor => User.Identity?.Name is { Length: > 0 and <= 120 } name ? name : ActorId.ToString();
        public RoleController(IRoleService roleService) { RoleService = roleService; }

        [HttpGet]
        public async Task<IActionResult> GetAsync() => Ok(await RoleService.GetAllAsync());

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            if (pageNumber is < 1 or > 100000 || pageSize is not (5 or 10 or 20 or 40 or 80) || (searchKeyword?.Length ?? 0) > 100)
                return BadRequest(new { Message = "Enter valid role paging options." });
            var tuple = await RoleService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var orderNumber = (pageNumber - 1) * pageSize + 1;
            return Ok(new { Data = tuple.Item1.Select(role => new { role.RoleId, role.Description, role.Name, OrderNumber = orderNumber++ }), Total = tuple.Item2 });
        }

        [HttpPost]
        public async Task<IActionResult> InsertAsync(RoleWriteRequest request, CancellationToken cancellationToken) =>
            WriteResponse(await RoleService.WriteAsync(UserOperation.Create, 0, request, ActorId, Actor, cancellationToken), true);

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(RoleWriteRequest request, CancellationToken cancellationToken) =>
            WriteResponse(await RoleService.WriteAsync(UserOperation.Update, request.RoleId, request, ActorId, Actor, cancellationToken));

        [HttpDelete("{roleId:int}")]
        public async Task<IActionResult> DeleteAsync(int roleId, CancellationToken cancellationToken) =>
            WriteResponse(await RoleService.WriteAsync(UserOperation.Delete, roleId, null, ActorId, Actor, cancellationToken));

        private IActionResult WriteResponse(RoleWriteResult result, bool created = false) => result.Status switch
        {
            UserWriteStatus.Invalid => BadRequest(new { result.Message }),
            UserWriteStatus.Conflict => Conflict(new { result.Message }),
            UserWriteStatus.NotFound => NotFound(new { result.Message }),
            UserWriteStatus.Forbidden => Forbid(),
            _ => created ? StatusCode(201, result.RoleId) : Ok()
        };
    }
}
