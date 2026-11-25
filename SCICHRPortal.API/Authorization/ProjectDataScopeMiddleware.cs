using System.Security.Claims;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Securities;

namespace SCICHRPortal.API.Authorization;

public sealed class ProjectDataScopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IProjectAccessService access, ProjectDataScope scope)
    {
        scope.Access = new(true);
        var actorId = context.User.Identity?.IsAuthenticated == true &&
            int.TryParse(context.User.FindFirstValue(ClaimTypes.Sid), out var id) ? id : 0;
        scope.Access = await access.GetAccessAsync(actorId, context.RequestAborted);
        await next(context);
    }
}
