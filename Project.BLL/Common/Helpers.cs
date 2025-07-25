using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project.BLL.Connection;
using Project.BLL.Services.Authorize;
using Project.DTO.DB;
using Project.DTO.Model;

namespace Project.BLL.Common
{
    public class Helpers
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;

        public Helpers(IConfiguration configuration, IHttpContextAccessor context)
        {
            _configuration = configuration;
            _context = context;
        }



        public UserTokenModel GetUserTokenModel(TB_ADM_USUARIO user)
        {
            using var _repository = new RepoDataBase(_configuration, _context);

            var displayName = user.NOME.Split(' ');
            return new UserTokenModel()
            {
                IsAuthenticated = true,
                Access_token = TokenService.GenerateToken(user),
                User = new UserModel()
                {
                    uuid = user.ID.ToString(),
                    role = "admin", // admin / user / staff
                    userName = user.USUARIO,
                    loginRedirectUrl = user.FORCE_ALTERAR_SENHA ? "/update-password" : "/dashboard",
                    from = _repository.GetInfo().GetDbConnection().Database,
                    data = new DataModel()
                    {
                        displayName = displayName.Length > 1 ? displayName.FirstOrDefault() + " " + displayName.LastOrDefault() : displayName.FirstOrDefault(),
                        email = user.EMAIL,
                        photoURL = null,
                        shortcuts = null,
                        settings = null,  //AQUI VAI O TEMA,
                        modules = null, // ARQUI VAI A LISTA DOS MODULOS
                    }
                }
            };
        }

    }
}

