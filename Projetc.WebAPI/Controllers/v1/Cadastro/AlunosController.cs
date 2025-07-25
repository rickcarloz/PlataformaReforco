using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.BLL.Common;
using Project.BLL.Connection;
using Project.BLL.Services.Authorize;
using Project.DTO.Common;
using Project.DTO.DB;
using System.Linq;

namespace Projetc.WebAPI.Controllers.v1.Admin
{
    [ApiController, Route("v1/Admin/Alunos"), /*Authorize*/] // Ative o Authorize conforme necessário
    public class AlunosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;

        public AlunosController(IConfiguration configuration, IHttpContextAccessor context, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _context = context;
            _loggerFactory = loggerFactory;
        }

        [HttpGet, Produces("application/json", Type = typeof(PaginateModel<AlunoDto>))]
        public ObjectResult Get([FromQuery] FilterBase<AlunoDto> model, [FromQuery] Guid? usuarioId = null)
        {
            model ??= new FilterBase<AlunoDto>();

            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Alunos>(x => x.ATIVO)
                .Include(x => x.AlunosFK)
                .Include(x => x.ProfessoresFK)
                .Include(x => x.TurmasFK)
                .AsNoTracking()
                .WhereIf(usuarioId.HasValue, x => x.ProfessorId == usuarioId.Value)
                .Select(x => new AlunoDto
                {
                    ID = x.AlunoId,
                    Nome = x.AlunosFK.NOME,
                    Email = x.AlunosFK.EMAIL,
                    Usuario = x.AlunosFK.USUARIO,
                    TurmaId = x.TurmaId,
                    TurmaNome = x.TurmasFK.Serie,
                    ProfessorId = x.ProfessorId,
                    ProfessorNome = x.ProfessoresFK.NOME
                })
                .ToList();

            return Ok(Paginate.List<AlunoDto>(model, data.OrderBy(x => x.Nome).ToList()));
        }

        [HttpGet, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<AlunoEditDto>))]
        public ObjectResult GetById(Guid Id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Alunos>(x => x.ID == Id)
                .Include(x => x.AlunosFK)
                .AsNoTracking()
                .FirstOrDefault();

            if (data == null)
            {
                return NotFound(new ReturnApiData<AlunoEditDto> { Success = false, Message = "Não encontrado" });
            }

            var alunoDto = new AlunoEditDto
            {
                ID = data.ID,
                NOME = data.AlunosFK.NOME,
                EMAIL = data.AlunosFK.EMAIL,
                USUARIO = data.AlunosFK.USUARIO,
                TurmaId = data.TurmaId
            };

            return Ok(new ReturnApiData<AlunoEditDto> { Success = true, Data = alunoDto });
        }

        [HttpGet, Route("Turma/{turmaId:guid}"), Produces("application/json", Type = typeof(PaginateModel<AlunoDto>))]
        public ObjectResult GetByTurma(Guid turmaId)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Alunos>(x => x.ATIVO && x.TurmaId == turmaId)
                .Include(x => x.AlunosFK)
                .Include(x => x.ProfessoresFK)
                .Include(x => x.TurmasFK)
                .AsNoTracking()
                .Select(x => new AlunoDto
                {
                    ID = x.ID,
                    Nome = x.AlunosFK.NOME,
                    Email = x.AlunosFK.EMAIL,
                    Usuario = x.AlunosFK.USUARIO,
                    TurmaId = x.TurmaId,
                    TurmaNome = x.TurmasFK.Serie,
                    ProfessorId = x.ProfessorId,
                    ProfessorNome = x.ProfessoresFK.NOME
                })
                .ToList();

            return Ok(Paginate.List<AlunoDto>(new FilterBase<AlunoDto>(), data.OrderBy(x => x.Nome).ToList()));
        }

        [HttpPost, Produces("application/json", Type = typeof(ReturnApiData<object>))]
        public ObjectResult Post([FromBody] AlunoCadastroDto model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            if (!ModelState.IsValid)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = new SerializableError(ModelState)
                });
            }
            try
            {
                // 1. Cria usuário aluno
                var usuario = new TB_ADM_USUARIO
                {
                    USUARIO = model.USUARIO,
                    NOME = model.NOME,
                    EMAIL = model.EMAIL,
                    Professor = false,
                    ATIVO = true
                };
                var senha = "123";//PasswordService.RadomPassword();
                PasswordService.CreatePasswordHash(senha, out string passwordHash, out string passwordSalt);
                usuario.PASSWORD_HASH = passwordHash;
                usuario.PASSWORD_SALT = passwordSalt;
                usuario.FORCE_ALTERAR_SENHA = true;

                _repository.Add(usuario);

                // 2. Cria relação em Alunos
                var relacao = new Alunos
                {
                    AlunoId = usuario.ID,
                    ProfessorId = model.ProfessorId,
                    TurmaId = model.TurmaId
                };
                _repository.Add(relacao);

                return Ok(new ReturnApiData<object> { Success = true, Data = usuario });
            }
            catch (Exception e)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }
        }

        [HttpPatch, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<Alunos>))]
        public ObjectResult Patch(Guid id, [FromBody] JsonPatchDocument<AlunoEditDto> model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);

            if (model == null)
            {
                return BadRequest(new ReturnApiData<Alunos> { Success = false, Message = "Dados inválidos" });
            }

            var data = _repository.Get<Alunos>(x => x.ID == id)
                .Include(x => x.AlunosFK)
                .FirstOrDefault();

            if (data == null)
            {
                return NotFound(new ReturnApiData<Alunos> { Success = false, Message = "Não encontrado" });
            }

            try
            {
                // Aplicar as mudanças no usuário (AlunosFK)
                foreach (var operation in model.Operations)
                {
                    switch (operation.path.ToLower())
                    {
                        case "/nome":
                            data.AlunosFK.NOME = operation.value?.ToString();
                            break;
                        case "/email":
                            data.AlunosFK.EMAIL = operation.value?.ToString();
                            break;
                        case "/usuario":
                            data.AlunosFK.USUARIO = operation.value?.ToString();
                            break;
                        case "/turmaid":
                            if (Guid.TryParse(operation.value?.ToString(), out Guid turmaId))
                            {
                                data.TurmaId = turmaId;
                            }
                            break;
                    }
                }

                _repository.Edit(data);
                return Ok(new ReturnApiData<Alunos> { Success = true, Data = data });
            }
            catch (Exception e)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }
        }

        [HttpDelete, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<Alunos>))]
        public ObjectResult Delete(Guid id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<Alunos>(x => x.ID == id)
                .Include(x => x.AlunosFK)
                .FirstOrDefault();

            if (data == null)
            {
                return NotFound(new ReturnApiData<Alunos> { Success = false, Message = "Não encontrado" });
            }

            try
            {
                // Marcar como inativo tanto a relação quanto o usuário
                _repository.SetDeleted(data);
                _repository.SetDeleted(data.AlunosFK);
                
                return Ok(new ReturnApiData<Alunos> { Success = true, Data = data });
            }
            catch (Exception e)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }
        }

        public class AlunoCadastroDto
        {
            public string USUARIO { get; set; }
            public string NOME { get; set; }
            public string EMAIL { get; set; }
            public Guid ProfessorId { get; set; }
            public Guid TurmaId { get; set; }
        }

        public class AlunoEditDto
        {
            public Guid ID { get; set; }
            public string NOME { get; set; }
            public string EMAIL { get; set; }
            public string USUARIO { get; set; }
            public Guid TurmaId { get; set; }
        }

        public class AlunoDto
        {
            public Guid ID { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Usuario { get; set; }
            public Guid TurmaId { get; set; }
            public string TurmaNome { get; set; }
            public Guid ProfessorId { get; set; }
            public string ProfessorNome { get; set; }
        }
    }
}