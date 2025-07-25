using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.BLL.Common;
using Project.BLL.Connection;
using Project.BLL.Services.Authorize;
using Project.DTO.Common;
using Project.DTO.DB;

namespace Projetc.WebAPI.Controllers.v1.Admin
{
    [ApiController, Route("v1/Admin/Usuario"), /*Authorize*/] //TODO: ATIVAR O AUTORIZE
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;

        public UsuarioController(IConfiguration configuration, IHttpContextAccessor context, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _context = context;
            _loggerFactory = loggerFactory;

        }


        [HttpGet, Produces("application/json", Type = typeof(PaginateModel<TB_ADM_USUARIO>))]
        public ObjectResult Get([FromQuery] FilterBase<TB_ADM_USUARIO> model)
        {
            model ??= new FilterBase<TB_ADM_USUARIO>();

            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<TB_ADM_USUARIO>(x => x.ATIVO)
                .AsNoTracking()
                .WhereIf(!string.IsNullOrEmpty(model.Filter.USUARIO), x => x.USUARIO == model.Filter.USUARIO)
                .ToList();

            return Ok(Paginate.List(model, data.OrderBy(x => x.NOME).ToList()));

        }


        [HttpGet, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<TB_ADM_USUARIO>))]
        public ObjectResult GetById(Guid Id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<TB_ADM_USUARIO>(x => x.ID == Id).AsNoTracking().FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<TB_ADM_USUARIO> { Success = false, Message = "Não encontrado" });
            }


            return Ok(new ReturnApiData<TB_ADM_USUARIO> { Success = true, Data = data });
        }



        [HttpPost, Produces("application/json", Type = typeof(ReturnApiData<TB_ADM_USUARIO>))]
        public ObjectResult Post([FromBody] TB_ADM_USUARIO model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
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
                var senha = PasswordService.RadomPassword();
                PasswordService.CreatePasswordHash(senha, out string passwordHash, out string passwordSalt);

                model.PASSWORD_HASH = passwordHash;
                model.PASSWORD_SALT = passwordSalt;
                model.USUARIO = model.USUARIO;
                model.FORCE_ALTERAR_SENHA = true;

                // TODO: Função para disparar e-mail para o usuário trocar a senha

                _repository.Add(model);
                return Ok(new ReturnApiData<TB_ADM_USUARIO> { Success = true, Data = model });
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



        [HttpPatch, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<TB_ADM_USUARIO>))]
        public ObjectResult Patch(Guid id, [FromBody] JsonPatchDocument<TB_ADM_USUARIO> model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);

            if (model == null)
            {
                return BadRequest(new ReturnApiData<TB_ADM_USUARIO> { Success = false, Message = "Não encontrado" });
            }
            var data = _repository.Get<TB_ADM_USUARIO>(x => x.ID == id).FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<TB_ADM_USUARIO> { Success = false, Message = "Não encontrado" });
            }

            try
            {

                _repository.Patch(model, data);
                return Ok(new ReturnApiData<TB_ADM_USUARIO> { Success = true, Data = data });
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


        [HttpDelete, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<TB_ADM_USUARIO>))]
        public ObjectResult Delete(Guid id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<TB_ADM_USUARIO>(x => x.ID == id).FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<TB_ADM_USUARIO> { Success = false, Message = "Não encontrado" });
            }
            try
            {
                _repository.SetDeleted(data);
                return Ok(new ReturnApiData<TB_ADM_USUARIO> { Success = true, Data = data });
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
