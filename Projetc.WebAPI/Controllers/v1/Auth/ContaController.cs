using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.BLL.Common;
using Project.BLL.Connection;
using Project.BLL.Services.Authorize;
using Project.DTO.Common;
using Project.DTO.DB;
using Project.DTO.Model;
using SignInStatus = Microsoft.AspNet.Identity.Owin.SignInStatus;

namespace Projetc.WebAPI.Controllers.v1.Auth
{

    [ApiController, Route("v1/Auth/Conta"), Authorize]
    public class ContaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;

        public ContaController(IConfiguration configuration, IHttpContextAccessor context, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _context = context;
            _loggerFactory = loggerFactory;
        }


        [HttpPost, Route("Login"), AllowAnonymous, Consumes("application/x-www-form-urlencoded")]
        public async Task<ObjectResult> Login([FromForm] UserLoginModel model)
        {
            // new Helpers(_configuration, _context).InitConfig();
            using var _repository = new RepoDataBase(_configuration, _context);
            SignInStatus result = SignInStatus.Failure;
            var user = await _repository.Get<TB_ADM_USUARIO>(x => x.USUARIO == model.UserName).AsNoTracking().FirstOrDefaultAsync();
            if (user != null)
            {

                result = user.ATIVO ? AuthenticationService.SingInLocal(model.Password, user.PASSWORD_HASH, user.PASSWORD_SALT) : SignInStatus.LockedOut;
                return result switch
                {
                    SignInStatus.Success => Ok(new Helpers(_configuration, _context).GetUserTokenModel(user)),
                    SignInStatus.LockedOut => BadRequest(new UserTokenModel() { IsAuthenticated = false, Message = "Esta conta foi bloqueada, tente novamente mais tarde" }),
                    _ => BadRequest(new UserTokenModel() { IsAuthenticated = false, Message = "As credenciais do usuário não conferem...." }),
                };
            }

            return NotFound(new UserTokenModel()
            {
                IsAuthenticated = false,
                Message = "Usuário ou senha inválidos"
            });

        }


        [HttpPost, Route("ValidarToken"), AllowAnonymous]
        public async Task<ObjectResult> ValidarToken([FromBody] UserLoginModel model)
        {
            var UserId = TokenService.ValidateToken(model.AccessToken);
            if (UserId != Guid.Empty)
            {
                using var _repository = new RepoDataBase(_configuration, _context);
                SignInStatus result = SignInStatus.Failure;
                var user = await _repository.Get<TB_ADM_USUARIO>(x => x.ID == UserId).AsNoTracking().FirstOrDefaultAsync();

                if (user != null)
                {
                    if (!user.ATIVO)
                    {
                        result = SignInStatus.LockedOut;
                    }
                    else
                    {
                        result = SignInStatus.Success;
                    }

                    return result switch
                    {
                        SignInStatus.Success => Ok(new Helpers(_configuration, _context).GetUserTokenModel(user)),
                        SignInStatus.LockedOut => BadRequest(new UserTokenModel() { IsAuthenticated = false, Message = "Esta conta foi bloqueada, tente novamente mais tarde" }),
                        _ => BadRequest(new UserTokenModel() { IsAuthenticated = false, Message = "As credenciais do usuário não conferem...." }),
                    };

                }
            }
            return NotFound(new UserTokenModel() { IsAuthenticated = false, Message = "Token expirado" });
        }


        [HttpPost, Route("TrocarSenha")]
        public ObjectResult TrocarSenha([FromBody] UserChangePassword model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ReturnApiData<object>()
                {
                    Success = false,
                    Message = new SerializableError(ModelState)
                });
            }

            try
            {
                using var _repository = new RepoDataBase(_configuration, _context);
                var user = _repository.Get<TB_ADM_USUARIO>(x => x.ID == model.UserId).FirstOrDefault();
                if (user != null)
                {


                    bool isValid = PasswordService.VerifyPasswordHash(model.OldPassword, user.PASSWORD_HASH, user.PASSWORD_SALT);
                    if (isValid)
                    {
                        PasswordService.CreatePasswordHash(model.NewPassword, out string passwordHash, out string passwordSalt);
                        user.PASSWORD_HASH = passwordHash;
                        user.PASSWORD_SALT = passwordSalt;
                        user.FORCE_ALTERAR_SENHA = false;
                        _repository.Edit(user);

                        return Ok(new ReturnApiData<TB_ADM_USUARIO> { Success = true, Data = user });
                    }
                    else
                    {
                        return BadRequest(new ReturnApiData<object>()
                        {
                            Success = false,
                            Message = "Senha atual não é válida"
                        });
                    }
                }

                return BadRequest(new ReturnApiData<object>()
                {
                    Success = false,
                    Message = "Usuário não encontrado"
                });
            }
            catch (Exception e)
            {
                return BadRequest(new ReturnApiData<object>()
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }

        }


        [HttpPost, Route("ResetSenha"), AllowAnonymous]
        public ObjectResult ResetSenha([FromBody] UserResetPassword model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ReturnApiData<object>()
                {
                    Success = false,
                    Message = new SerializableError(ModelState)
                });
            }

            try
            {
                using var _repository = new RepoDataBase(_configuration, _context);
                var user = _repository.Get<TB_ADM_USUARIO>(x => x.USUARIO == model.UserName).FirstOrDefault();
                if (user != null)
                {



                    var senha = PasswordService.RadomPassword();
                    PasswordService.CreatePasswordHash(senha, out string passwordHash, out string passwordSalt);
                    user.PASSWORD_HASH = passwordHash;
                    user.PASSWORD_SALT = passwordSalt;
                    user.FORCE_ALTERAR_SENHA = true;
                    _repository.Edit(user);


                    //CONFIGURAR FUNCAO PARA ENVIO DE EMAIL //
                    return Ok("CONFIGURAR FUNCAO PARA ENVIO DE EMAIL");
                }

                return BadRequest(new ReturnApiData<object>()
                {
                    Success = false,
                    Message = "Usuário não encontrado"
                });
            }
            catch (Exception e)
            {
                return BadRequest(new ReturnApiData<object>()
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }

        }

    }

}
