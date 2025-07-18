using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace PlataformaReforco.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class TipoUsuarioAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _tipoUsuario;
        public TipoUsuarioAuthorizeAttribute(string tipoUsuario)
        {
            _tipoUsuario = tipoUsuario;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated || !user.HasClaim(c => c.Type == "TipoUsuario" && c.Value == _tipoUsuario))
            {
                context.Result = new ForbidResult();
            }
        }
    }
} 