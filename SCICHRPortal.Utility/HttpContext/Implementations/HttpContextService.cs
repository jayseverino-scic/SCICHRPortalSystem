using Microsoft.AspNetCore.Http;
using SCICHRPortal.Utility.HttpContext.Interfaces;

namespace SCICHRPortal.Utility.HttpContext.Implementations
{
    public class HttpContextService : IHttpContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? GetUser()
        {
            return _httpContextAccessor.HttpContext!.Request.Headers["RequesterName"];
        }

        public string? GetReferrer()
        {
            return _httpContextAccessor.HttpContext!.Request.Headers["RequestOrigin"];
        }

        public string? GetSystem()
        {
            return _httpContextAccessor.HttpContext!.Request.Headers["RequestSystem"];
        }

    }
}
