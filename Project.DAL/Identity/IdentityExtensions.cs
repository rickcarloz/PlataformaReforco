using System.Security.Claims;
using System.Security.Principal;

namespace Project.DAL.Identity
{
    public static class IdentityExtensions
    {

        public static bool IsAdmin(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            return bool.TryParse(claimsIdentity.Claims.First(x => x.Type == "IsAdmin").Value, out bool isAdmin) ? isAdmin : false;
        }


        public static string GetEmail(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity.Claims.First(x => x.Type == ClaimTypes.Email)?.Value ?? "";

        }


        public static Guid GetId(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            return Guid.TryParse(claimsIdentity.Claims.First(x => x.Type == "Id").Value, out Guid Id) ? Id : Guid.Empty;

        }


        public static bool IsAgenteMKT(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            return bool.TryParse(claimsIdentity.Claims.First(x => x.Type == "IsAgenteMKT").Value, out Boolean value) ? value : false;

        }

    }
}
