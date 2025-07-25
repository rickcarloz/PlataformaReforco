using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.BLL.Common;
using Project.BLL.Connection;
using Project.DTO.Common;
using Project.DTO.DB;
using System.Linq;
using ProvasEntity = Project.DTO.DB.Provas;
using QuestoesEntity = Project.DTO.DB.Questoes;
using AlternativasEntity = Project.DTO.DB.Alternativas;

namespace Projetc.WebAPI.Controllers.v1.Provas
{
    [ApiController, Route("v1/Provas"), /*Authorize*/]
    public class ProvasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;

        public ProvasController(IConfiguration configuration, IHttpContextAccessor context, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _context = context;
            _loggerFactory = loggerFactory;
        }

        [HttpGet, Produces("application/json", Type = typeof(PaginateModel<ProvaDto>))]
        public ObjectResult Get([FromQuery] FilterBase<ProvaDto> model)
        {
            model ??= new FilterBase<ProvaDto>();

            using var _repository = new RepoDataBase(_configuration, _context);
            var provas = _repository.Get<ProvasEntity>(x => x.ATIVO)
                .Include(x => x.Turma)
                .Include(x => x.Professor)
                .AsNoTracking()
                .ToList();

            // Buscar quantidade de questões para cada prova
            var questoesPorProva = _repository.Get<QuestoesEntity>(q => q.ATIVO)
                .GroupBy(q => q.ProvaId)
                .Select(g => new { ProvaId = g.Key, Quantidade = g.Count() })
                .ToDictionary(x => x.ProvaId, x => x.Quantidade);

            var data = provas.Select(x => new ProvaDto
            {
                ID = x.ID,
                Titulo = x.Titulo,
                Descricao = x.Descricao,
                TurmaId = x.TurmaId,
                TurmaNome = x.Turma.Serie,
                ProfessorId = x.ProfessorId,
                ProfessorNome = x.Professor.NOME,
                TempoLimite = x.TempoLimite,
                Ativa = x.Ativa,
                QuantidadeQuestoes = questoesPorProva.ContainsKey(x.ID) ? questoesPorProva[x.ID] : 0
            }).ToList();

            return Ok(Paginate.List<ProvaDto>(model, data.OrderBy(x => x.Titulo).ToList()));
        }

        [HttpGet, Route("{id:guid}/Questoes"), Produces("application/json", Type = typeof(ReturnApiData<List<QuestaoDto>>))]
        public ObjectResult GetQuestoes(Guid Id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            
            var questoes = _repository.Get<QuestoesEntity>(q => q.ATIVO && q.ProvaId == Id)
                .Include(q => q.Alternativas.Where(a => a.ATIVO))
                .AsNoTracking()
                .OrderBy(q => q.Ordem)
                .ToList();

            var questoesDto = questoes.Select(q => new QuestaoDto
            {
                ID = q.ID,
                Enunciado = q.Enunciado,
                Ordem = q.Ordem,
                Pontos = q.Pontos,
                Alternativas = q.Alternativas.Select(a => new AlternativaDto
                {
                    ID = a.ID,
                    Texto = a.Texto,
                    Letra = a.Letra,
                    Correta = a.Correta
                }).ToList()
            }).ToList();

            return Ok(new ReturnApiData<List<QuestaoDto>> { Success = true, Data = questoesDto });
        }

        [HttpGet, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<ProvaCompletaDto>))]
        public ObjectResult GetById(Guid Id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<ProvasEntity>(x => x.ID == Id)
                .Include(x => x.Turma)
                .Include(x => x.Professor)
                .AsNoTracking()
                .FirstOrDefault();

            if (data == null)
            {
                return NotFound(new ReturnApiData<ProvaCompletaDto> { Success = false, Message = "Prova não encontrada" });
            }

            var questoes = _repository.Get<QuestoesEntity>(q => q.ATIVO && q.ProvaId == Id)
                .Include(q => q.Alternativas.Where(a => a.ATIVO))
                .AsNoTracking()
                .OrderBy(q => q.Ordem)
                .ToList();

            var provaCompleta = new ProvaCompletaDto
            {
                ID = data.ID,
                Titulo = data.Titulo,
                Descricao = data.Descricao,
                TurmaId = data.TurmaId,
                TurmaNome = data.Turma.Serie,
                ProfessorId = data.ProfessorId,
                ProfessorNome = data.Professor.NOME,
                TempoLimite = data.TempoLimite,
                Ativa = data.Ativa,
                Questoes = questoes.Select(q => new QuestaoDto
                {
                    ID = q.ID,
                    Enunciado = q.Enunciado,
                    Ordem = q.Ordem,
                    Pontos = q.Pontos,
                    Alternativas = q.Alternativas.Select(a => new AlternativaDto
                    {
                        ID = a.ID,
                        Texto = a.Texto,
                        Letra = a.Letra,
                        Correta = a.Correta
                    }).ToList()
                }).ToList()
            };

            return Ok(new ReturnApiData<ProvaCompletaDto> { Success = true, Data = provaCompleta });
        }

        [HttpGet, Route("{id:guid}/Detalhes"), Produces("application/json", Type = typeof(ReturnApiData<ProvaDetalhesDto>))]
        public ObjectResult GetDetalhes(Guid Id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<ProvasEntity>(x => x.ID == Id)
                .Include(x => x.Turma)
                .Include(x => x.Professor)
                .AsNoTracking()
                .FirstOrDefault();

            if (data == null)
            {
                return NotFound(new ReturnApiData<ProvaDetalhesDto> { Success = false, Message = "Prova não encontrada" });
            }

            var quantidadeQuestoes = _repository.Get<QuestoesEntity>(q => q.ATIVO && q.ProvaId == Id).Count();

            var provaDetalhes = new ProvaDetalhesDto
            {
                ID = data.ID,
                Titulo = data.Titulo,
                Descricao = data.Descricao,
                TurmaId = data.TurmaId,
                TurmaNome = data.Turma.Serie,
                ProfessorId = data.ProfessorId,
                ProfessorNome = data.Professor.NOME,
                TempoLimite = data.TempoLimite,
                Ativa = data.Ativa,
                QuantidadeQuestoes = quantidadeQuestoes,
                DataCriacao = data.DATA_CRIACAO.DateTime
            };

            return Ok(new ReturnApiData<ProvaDetalhesDto> { Success = true, Data = provaDetalhes });
        }

        [HttpPost, Produces("application/json", Type = typeof(ReturnApiData<ProvasEntity>))]
        public ObjectResult Post([FromBody] CriarProvaDto model)
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

            if (model.Questoes.Count > 10)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = "A prova não pode ter mais de 10 questões"
                });
            }

            try
            {
                // Criar a prova
                var prova = new ProvasEntity
                {
                    Titulo = model.Titulo,
                    Descricao = model.Descricao,
                    TurmaId = model.TurmaId,
                    ProfessorId = model.ProfessorId,
                    TempoLimite = model.TempoLimite,
                    Ativa = true
                };

                _repository.Add(prova);

                // Criar as questões e alternativas
                for (int i = 0; i < model.Questoes.Count; i++)
                {
                    var questaoDto = model.Questoes[i];
                    var questao = new QuestoesEntity
                    {
                        ProvaId = prova.ID,
                        Enunciado = questaoDto.Enunciado,
                        Ordem = i + 1,
                        Pontos = questaoDto.Pontos
                    };

                    _repository.Add(questao);

                    // Criar as alternativas
                    for (int j = 0; j < questaoDto.Alternativas.Count; j++)
                    {
                        var alternativaDto = questaoDto.Alternativas[j];
                        var alternativa = new AlternativasEntity
                        {
                            QuestaoId = questao.ID,
                            Texto = alternativaDto.Texto,
                            Letra = alternativaDto.Letra,
                            Correta = alternativaDto.Correta
                        };

                        _repository.Add(alternativa);
                    }
                }

                return Ok(new ReturnApiData<ProvasEntity> { Success = true, Data = prova });
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

        [HttpPatch, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<ProvasEntity>))]
        public ObjectResult Patch(Guid id, [FromBody] JsonPatchDocument<ProvasEntity> model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);

            if (model == null)
            {
                return BadRequest(new ReturnApiData<ProvasEntity> { Success = false, Message = "Dados inválidos" });
            }

            var data = _repository.Get<ProvasEntity>(x => x.ID == id).FirstOrDefault();
            if (data == null)
            {
                return NotFound(new ReturnApiData<ProvasEntity> { Success = false, Message = "Prova não encontrada" });
            }

            try
            {
                _repository.Patch(model, data);
                return Ok(new ReturnApiData<ProvasEntity> { Success = true, Data = data });
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

        [HttpDelete, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<ProvasEntity>))]
        public ObjectResult Delete(Guid id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<ProvasEntity>(x => x.ID == id).FirstOrDefault();

            if (data == null)
            {
                return NotFound(new ReturnApiData<ProvasEntity> { Success = false, Message = "Prova não encontrada" });
            }

            try
            {
                _repository.SetDeleted(data);
                return Ok(new ReturnApiData<ProvasEntity> { Success = true, Data = data });
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

        public class ProvaDto
        {
            public Guid ID { get; set; }
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Guid TurmaId { get; set; }
            public string TurmaNome { get; set; }
            public Guid ProfessorId { get; set; }
            public string ProfessorNome { get; set; }
            public int TempoLimite { get; set; }
            public bool Ativa { get; set; }
            public int QuantidadeQuestoes { get; set; }
        }

        public class ProvaCompletaDto
        {
            public Guid ID { get; set; }
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Guid TurmaId { get; set; }
            public string TurmaNome { get; set; }
            public Guid ProfessorId { get; set; }
            public string ProfessorNome { get; set; }
            public int TempoLimite { get; set; }
            public bool Ativa { get; set; }
            public List<QuestaoDto> Questoes { get; set; } = new();
        }

        public class QuestaoDto
        {
            public Guid ID { get; set; }
            public string Enunciado { get; set; }
            public int Ordem { get; set; }
            public int Pontos { get; set; }
            public List<AlternativaDto> Alternativas { get; set; } = new();
        }

        public class AlternativaDto
        {
            public Guid ID { get; set; }
            public string Texto { get; set; }
            public string Letra { get; set; }
            public bool Correta { get; set; }
        }

        public class CriarProvaDto
        {
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Guid TurmaId { get; set; }
            public Guid ProfessorId { get; set; }
            public int TempoLimite { get; set; }
            public List<CriarQuestaoDto> Questoes { get; set; } = new();
        }

        public class CriarQuestaoDto
        {
            public string Enunciado { get; set; }
            public int Pontos { get; set; } = 1;
            public List<CriarAlternativaDto> Alternativas { get; set; } = new();
        }

        public class CriarAlternativaDto
        {
            public Guid ID { get; set; }
            public string Texto { get; set; }
            public string Letra { get; set; }
            public bool Correta { get; set; }
        }

        public class ProvaDetalhesDto
        {
            public Guid ID { get; set; }
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Guid TurmaId { get; set; }
            public string TurmaNome { get; set; }
            public Guid ProfessorId { get; set; }
            public string ProfessorNome { get; set; }
            public int TempoLimite { get; set; }
            public bool Ativa { get; set; }
            public int QuantidadeQuestoes { get; set; }
            public DateTime DataCriacao { get; set; }
        }
    }
} 