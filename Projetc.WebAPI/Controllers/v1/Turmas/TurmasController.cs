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
    [ApiController, Route("v1/Turmas"), /*Authorize*/] //TODO: ATIVAR O AUTORIZE
    public class TurmasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;

        public TurmasController(IConfiguration configuration, IHttpContextAccessor context, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _context = context;
            _loggerFactory = loggerFactory;

        }


        [HttpGet, Produces("application/json", Type = typeof(PaginateModel<Turmas>))]
        public ObjectResult Get([FromQuery] FilterBase<Turmas> model, [FromQuery] Guid? usuarioId = null)
        {
            model ??= new FilterBase<Turmas>();

            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Turmas>(x => x.ATIVO)
                .AsNoTracking()
                .WhereIf(!string.IsNullOrEmpty(model.Filter.Serie), x => x.Serie == model.Filter.Serie)
                .WhereIf(usuarioId.HasValue, x => x.UsuarioId == usuarioId.Value)
                .ToList();

            return Ok(Paginate.List(model, data.OrderBy(x => x.Serie).ToList()));
        }

        [HttpGet, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<Turmas>))]
        public ObjectResult GetById(Guid Id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Turmas>(x => x.ID == Id).AsNoTracking().FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<Turmas> { Success = false, Message = "Não encontrado" });
            }

            return Ok(new ReturnApiData<Turmas> { Success = true, Data = data });
        }


        [HttpPost, Produces("application/json", Type = typeof(ReturnApiData<Turmas>))]
        public ObjectResult Post([FromBody] Turmas model)
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
                _repository.Add(model);
                return Ok(new ReturnApiData<Turmas> { Success = true, Data = model });
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


        [HttpPatch, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<Turmas>))]
        public ObjectResult Patch(Guid id, [FromBody] JsonPatchDocument<Turmas> model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);

            if (model == null)
            {
                return BadRequest(new ReturnApiData<Turmas> { Success = false, Message = "Não encontrado" });
            }
            var data = _repository.Get<Turmas>(x => x.ID == id).FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<Turmas> { Success = false, Message = "Não encontrado" });
            }

            try
            {

                _repository.Patch(model, data);
                return Ok(new ReturnApiData<Turmas> { Success = true, Data = data });
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


        [HttpDelete, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<Turmas>))]
        public ObjectResult Delete(Guid id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Turmas>(x => x.ID == id).FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<Turmas> { Success = false, Message = "Não encontrado" });
            }
            try
            {
                _repository.SetDeleted(data);
                return Ok(new ReturnApiData<Turmas> { Success = true, Data = data });
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
