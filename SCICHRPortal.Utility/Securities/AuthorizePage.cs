using Microsoft.AspNetCore.Mvc.Razor;

namespace SCICHRPortal.Utility.Securities
{
    public abstract class AuthorizePage<T> : RazorPage<T>
    {
        public async Task<bool> HasRoleAsync(string roleName)
        {
            return User.Claims.Any(u => u.Value == roleName);
        }

        public async Task<bool> HasRoleByArrayAsync(string[] roleNames)
        {
            var hasRole = false;
            foreach (var roleName in roleNames)
            {
                hasRole =  User.Claims.Any(u => u.Value == roleName);
                if (hasRole)
                    break;
            }

            return hasRole;
           
        }


        public async Task<bool> IsAuthorize()
        {
            return User.Identity!.IsAuthenticated;
        }
    }
}
