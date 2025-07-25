using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebUI.Filters
{
    public class AuthFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToPageResult("/Login");
            }
        }
    }
}
