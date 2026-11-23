using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class CurrentAdministratorAttribute : TypeFilterAttribute
{
    public CurrentAdministratorAttribute() : base(typeof(CurrentAdministratorFilter)) { }
}

public sealed class CurrentAdministratorFilter(IUserManagementService users) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!int.TryParse(context.HttpContext.User.FindFirstValue(ClaimTypes.Sid), out var id) || id <= 0)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        if (!await users.CanManageUsersAsync(id, context.HttpContext.RequestAborted))
            context.Result = new ForbidResult();
    }
}
