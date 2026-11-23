using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Authorization;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Users;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService UserService { get; }
        private IUserManagementService Management { get; }
        private int ActorId => int.TryParse(User.FindFirstValue(ClaimTypes.Sid), out var id) ? id : 0;
        private string Actor => User.Identity?.Name is { Length: > 0 and <= 120 } name ? name : ActorId.ToString();
        public UserController(IUserService userService, IUserManagementService management)
        {
            UserService = userService;
            Management = management;
        }

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] UserPageQuery query, CancellationToken cancellationToken)
        {
            if (!query.IsValid()) return BadRequest(new { Message = "Enter valid user filters and paging options." });
            return Ok(await Management.GetPageAsync(query, cancellationToken));
        }

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> DetailsAsync(int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0) return BadRequest(new { Message = "Select a valid user." });
            var account = await Management.GetDetailsAsync(userId, cancellationToken);
            return account == null ? NotFound(new { Message = "User was not found." }) : Ok(account);
        }

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("Roles")]
        public async Task<IActionResult> RolesAsync(CancellationToken cancellationToken) => Ok(await Management.GetRolesAsync(cancellationToken));

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            if (roleId <= 0) return BadRequest(new { Message = "Select a valid role." });
            var links = await UserService.GetByRoleIdsync(roleId);
            return Ok(links.Select(link => new { link.UserId, FullName = $"{link.User?.LastName}, {link.User?.FirstName}", link.User?.Email, link.User?.Username }));
        }

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("Email/{email}")]
        public async Task<IActionResult> GetByRoleId(string email)
        {
            var user = await UserService.GetByEmailAsync(email.Trim());
            return user == null ? NotFound(new { Message = "User was not found." }) : Ok(await Management.GetDetailsAsync(user.UserId, HttpContext.RequestAborted));
        }

        [HttpGet("GetAuthenticatedAsync")]
        public async Task<IActionResult> GetAuthenticatedAsync()
        {
            if (ActorId <= 0) return Unauthorized();
            var profile = await Management.GetAuthenticatedAsync(ActorId, HttpContext.RequestAborted);
            return profile == null ? Unauthorized() : Ok(profile);
        }

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("Unapproved/Filter")]
        public Task<IActionResult> GetUnApprovedUserAsync(int pageNumber, int pageSize, string? searchKeyword) => LegacyFilterAsync(pageNumber, pageSize, searchKeyword, "pending");
        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpGet("Approved/Filter")]
        public Task<IActionResult> GetApprovedUserAsync(int pageNumber, int pageSize, string? searchKeyword) => LegacyFilterAsync(pageNumber, pageSize, searchKeyword, "approved");
        private async Task<IActionResult> LegacyFilterAsync(int pageNumber, int pageSize, string? searchKeyword, string approval)
        {
            if (pageNumber is < 1 or > 100000 || pageSize is not (5 or 10 or 20 or 40 or 80)) return BadRequest(new { Message = "Enter valid paging options." });
            var query = new UserPageQuery { Start = (pageNumber - 1) * pageSize, Length = pageSize, Search = searchKeyword, Approval = approval };
            if (!query.IsValid()) return BadRequest(new { Message = "Enter valid paging options." });
            var page = await Management.GetPageAsync(query, HttpContext.RequestAborted);
            return Ok(new { Data = page.Data, Total = page.RecordsFiltered });
        }

        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpPost("Register")]
        public async Task<IActionResult> PostAsync(UserWriteRequest request, CancellationToken cancellationToken) => WriteResponse(await Management.WriteAsync(UserOperation.Create, 0, request, ActorId, Actor, cancellationToken), true);
        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> PutAsync(int userId, UserWriteRequest request, CancellationToken cancellationToken) => WriteResponse(await Management.WriteAsync(UserOperation.Update, userId, request, ActorId, Actor, cancellationToken));
        [HttpPut("Profile")]
        public async Task<IActionResult> ProfileAsync(UserProfileRequest request, CancellationToken cancellationToken) => WriteResponse(await Management.WriteAsync(UserOperation.Profile, ActorId, request, ActorId, Actor, cancellationToken));
        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpPut("Approved/{userId:int}")]
        public Task<IActionResult> ApprovedAsync(int userId, CancellationToken cancellationToken) => WriteAsync(UserOperation.Approve, userId, cancellationToken);
        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpPut("{userId:int}/Status/{status:bool}")]
        public Task<IActionResult> UpdateStatusAsync(int userId, bool status, CancellationToken cancellationToken) => WriteAsync(status ? UserOperation.Activate : UserOperation.Deactivate, userId, cancellationToken);
        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpDelete("{userId:int}")]
        public Task<IActionResult> DeleteAsync(int userId, CancellationToken cancellationToken) => WriteAsync(UserOperation.Delete, userId, cancellationToken);
        [Authorize(Roles = "Administrator"), CurrentAdministrator]
        [HttpPost("{userId:int}/ResetPassword")]
        public Task<IActionResult> ResetAccountPasswordAsync(int userId, CancellationToken cancellationToken) => WriteAsync(UserOperation.ResetPassword, userId, cancellationToken);

        [HttpPut("Reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestModel request)
        {
            if (ActorId <= 0) return Unauthorized();
            return WriteResponse(await Management.ChangeOwnPasswordAsync(ActorId, request.UserId, request.CurrentPassword, request.Password, Actor, HttpContext.RequestAborted));
        }
        private async Task<IActionResult> WriteAsync(UserOperation operation, int userId, CancellationToken cancellationToken) => WriteResponse(await Management.WriteAsync(operation, userId, null, ActorId, Actor, cancellationToken));
        private IActionResult WriteResponse(UserWriteResult result, bool created = false) => result.Status switch
        {
            UserWriteStatus.Invalid => BadRequest(new { result.Message }),
            UserWriteStatus.Conflict => Conflict(new { result.Message }),
            UserWriteStatus.NotFound => NotFound(new { result.Message }),
            UserWriteStatus.Forbidden => Forbid(),
            _ => StatusCode(created ? 201 : 200, new { result.UserId, result.Message, result.NotificationSent })
        };
    }
}
