using System.Security.Claims;

namespace LMS.Extensions
{
    public static class Identity_Extensions
    {
        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var claim = ((ClaimsIdentity?)claimsPrincipal.Identity)?
                .Claims
                .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return claim!.Value;
        }
    }
}