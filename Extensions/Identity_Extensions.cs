using System.Security.Claims;

namespace LMS.Extensions
{
    public static class Identity_Extensions
    {
        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new InvalidOperationException("claimsPrincipal context is null.");
            }

            var claim = ((ClaimsIdentity?)claimsPrincipal.Identity)?
                .Claims
                .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return claim!.Value;
        }
    }
}