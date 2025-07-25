using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.BLL.Common;
using Project.BLL.Connection;
using Project.DTO.Common;
using Project.DTO.DB;
using System.Text;
using System.Text.Json;
using System.Linq;
using ProvasAlunosEntity = Project.DTO.DB.ProvasAlunos;
using QuestoesEntity = Project.DTO.DB.Questoes;
using AlternativasEntity = Project.DTO.DB.Alternativas;
using RespostasAlunosEntity = Project.DTO.DB.RespostasAlunos;

namespace Projetc.WebAPI.Controllers.v1.Provas
{
    [ApiController, Route("v1/ProvasAlunos"), /*Authorize*/]
    public class ProvasAlunosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Project.BLL.Services.ChatGPTService _chatGPTService;

        public ProvasAlunosController(IConfiguration configuration, IHttpContextAccessor context, ILoggerFactory loggerFactory, Project.BLL.Services.ChatGPTService chatGPTService)
        {
            _configuration = configuration;
            _context = context;
            _loggerFactory = loggerFactory;
            _chatGPTService = chatGPTService;
        }

        [HttpGet, Produces("application/json", Type = typeof(PaginateModel<ProvaAlunoDto>))]
        public ObjectResult Get([FromQuery] FilterBase<ProvaAlunoDto> model)
        {
            model ??= new FilterBase<ProvaAlunoDto>();

            using var _repository = new RepoDataBase(_configuration, _context);
            var data = _repository.Get<ProvasAlunosEntity>(x => x.ATIVO)
                .Include(x => x.Prova)
                .Include(x => x.Aluno)
                .AsNoTracking()
                .Select(x => new ProvaAlunoDto
                {
                    ID = x.ID,
                    ProvaId = x.ProvaId,
                    ProvaTitulo = x.Prova.Titulo,
                    AlunoId = x.AlunoId,
                    AlunoNome = x.Aluno.NOME,
                    DataInicio = x.DataInicio,
                    DataFim = x.DataFim,
                    Pontuacao = x.Pontuacao,
                    Concluida = x.Concluida,
                    Aprovada = x.Aprovada,
                    RecomendacoesLiberadas = x.RecomendacoesLiberadas
                })
                .ToList();

            return Ok(Paginate.List<ProvaAlunoDto>(model, data.OrderByDescending(x => x.DataInicio).ToList()));
        }

        [HttpGet, Route("Aluno/{alunoId:guid}"), Produces("application/json", Type = typeof(PaginateModel<ProvaAlunoDto>))]
        public ObjectResult GetByAluno(Guid alunoId)
        {
            try
            {
                using var _repository = new RepoDataBase(_configuration, _context);
                
                // Verificar se o aluno existe
                var aluno = _repository.Get<TB_ADM_USUARIO>(x => x.ID == alunoId && x.ATIVO).FirstOrDefault();
                if (aluno == null)
                {
                    return NotFound(new ReturnApiData<object>
                    {
                        Success = false,
                        Message = "Aluno não encontrado"
                    });
                }

                var data = _repository.Get<ProvasAlunosEntity>(x => x.ATIVO && x.AlunoId == alunoId)
                    .Include(x => x.Prova)
                    .Include(x => x.Aluno)
                    .AsNoTracking()
                    .Select(x => new ProvaAlunoDto
                    {
                        ID = x.ID,
                        ProvaId = x.ProvaId,
                        ProvaTitulo = x.Prova != null ? x.Prova.Titulo : "Prova não encontrada",
                        AlunoId = x.AlunoId,
                        AlunoNome = x.Aluno != null ? x.Aluno.NOME : "Aluno não encontrado",
                        DataInicio = x.DataInicio,
                        DataFim = x.DataFim,
                        Pontuacao = x.Pontuacao,
                        Concluida = x.Concluida,
                        Aprovada = x.Aprovada,
                        RecomendacoesLiberadas = x.RecomendacoesLiberadas
                    })
                    .ToList();

                return Ok(Paginate.List<ProvaAlunoDto>(new FilterBase<ProvaAlunoDto>(), data.OrderByDescending(x => x.DataInicio).ToList()));
            }
            catch (Exception ex)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = $"Erro ao buscar provas do aluno: {ex.Message}"
                });
            }
        }

        [HttpGet, Route("TesteAluno/{alunoId:guid}"), Produces("application/json")]
        public ObjectResult TesteAluno(Guid alunoId)
        {
            try
            {
                using var _repository = new RepoDataBase(_configuration, _context);
                
                // Verificar se o aluno existe
                var aluno = _repository.Get<TB_ADM_USUARIO>(x => x.ID == alunoId && x.ATIVO).FirstOrDefault();
                
                if (aluno == null)
                {
                    return Ok(new { 
                        Success = false, 
                        Message = "Aluno não encontrado",
                        AlunoId = alunoId,
                        AlunosExistentes = _repository.Get<TB_ADM_USUARIO>(x => x.ATIVO && !x.Professor).Select(x => new { x.ID, x.NOME, x.USUARIO }).ToList()
                    });
                }

                // Verificar provas do aluno
                var provasAluno = _repository.Get<ProvasAlunosEntity>(x => x.ATIVO && x.AlunoId == alunoId).ToList();

                return Ok(new { 
                    Success = true, 
                    Aluno = new { aluno.ID, aluno.NOME, aluno.USUARIO, aluno.Professor },
                    ProvasAluno = provasAluno.Select(pa => new { pa.ID, pa.ProvaId, pa.AlunoId, pa.Concluida }).ToList(),
                    TotalProvas = provasAluno.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Erro: {ex.Message}",
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet, Route("{id:guid}"), Produces("application/json", Type = typeof(ReturnApiData<ProvaAlunoDto>))]
        public ObjectResult GetById(Guid id)
        {
            try
            {
                using var _repository = new RepoDataBase(_configuration, _context);
                
                var provaAluno = _repository.Get<ProvasAlunosEntity>(x => x.ID == id && x.ATIVO)
                    .Include(x => x.Prova)
                    .Include(x => x.Aluno)
                    .AsNoTracking()
                    .FirstOrDefault();

                if (provaAluno == null)
                {
                    return NotFound(new ReturnApiData<ProvaAlunoDto>
                    {
                        Success = false,
                        Message = "Prova não encontrada"
                    });
                }

                var resultado = new ProvaAlunoDto
                {
                    ID = provaAluno.ID,
                    ProvaId = provaAluno.ProvaId,
                    ProvaTitulo = provaAluno.Prova?.Titulo ?? "Prova não encontrada",
                    AlunoId = provaAluno.AlunoId,
                    AlunoNome = provaAluno.Aluno?.NOME ?? "Aluno não encontrado",
                    DataInicio = provaAluno.DataInicio,
                    DataFim = provaAluno.DataFim,
                    Pontuacao = provaAluno.Pontuacao,
                    Concluida = provaAluno.Concluida,
                    Aprovada = provaAluno.Aprovada,
                    RecomendacoesLiberadas = provaAluno.RecomendacoesLiberadas
                };

                return Ok(new ReturnApiData<ProvaAlunoDto>
                {
                    Success = true,
                    Data = resultado
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = $"Erro ao buscar prova: {ex.Message}"
                });
            }
        }

        [HttpPost, Route("Iniciar"), Produces("application/json", Type = typeof(ReturnApiData<ProvasAlunosEntity>))]
            public ObjectResult IniciarProva([FromBody] IniciarProvaDto model)
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
                    // Verificar se o aluno já iniciou esta prova
                    var provaExistente = _repository.Get<ProvasAlunosEntity>(x => x.ATIVO && x.ProvaId == model.ProvaId && x.AlunoId == model.AlunoId).FirstOrDefault();
                    if (provaExistente != null)
                    {
                        return BadRequest(new ReturnApiData<object>
                        {
                            Success = false,
                            Message = "Aluno já iniciou esta prova"
                        });
                    }

                    var provaAluno = new ProvasAlunosEntity
                    {
                        ProvaId = model.ProvaId,
                        AlunoId = model.AlunoId,
                        DataInicio = DateTime.Now,
                        Pontuacao = 0,
                        Concluida = false,
                        Aprovada = false
                    };

                    _repository.Add(provaAluno);
                    return Ok(new ReturnApiData<ProvasAlunosEntity> { Success = true, Data = provaAluno });
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

        [HttpPost, Route("TesteChatGPT"), Produces("application/json")]
        public async Task<ObjectResult> TesteChatGPT()
        {
            try
            {
                Console.WriteLine("=== DEBUG: TesteChatGPT chamado ===");
                
                var respostasErradas = new List<Project.BLL.Services.ChatGPTService.RespostaAnalise>
                {
                    new Project.BLL.Services.ChatGPTService.RespostaAnalise
                    {
                        NumeroQuestao = 1,
                        TextoQuestao = "Qual é a capital do Brasil?",
                        RespostaEscolhida = "São Paulo",
                        RespostaCorreta = "Brasília"
                    },
                    new Project.BLL.Services.ChatGPTService.RespostaAnalise
                    {
                        NumeroQuestao = 3,
                        TextoQuestao = "Quem descobriu o Brasil?",
                        RespostaEscolhida = "Cristóvão Colombo",
                        RespostaCorreta = "Pedro Álvares Cabral"
                    }
                };

                var recomendacoes = await _chatGPTService.GerarRecomendacoesAsync(
                    "História do Brasil", 
                    6.5, 
                    3, 
                    5, 
                    respostasErradas
                );

                Console.WriteLine($"=== DEBUG: Recomendações geradas: {recomendacoes} ===");

                return Ok(new ReturnApiData<object> 
                { 
                    Success = true, 
                    Data = new 
                    { 
                        Recomendacoes = recomendacoes,
                        Mensagem = "Teste do ChatGPT realizado com sucesso!"
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"=== DEBUG: Exception no teste ChatGPT: {e} ===");
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }
        }

        [HttpPost, Route("Finalizar"), Produces("application/json", Type = typeof(ReturnApiData<ProvasAlunosEntity>))]
        public async Task<ObjectResult> FinalizarProva([FromBody] FinalizarProvaDto model)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            
            // Debug: Logar o modelo recebido
            Console.WriteLine("=== DEBUG: FinalizarProva chamado ===");
            Console.WriteLine($"ProvaAlunoId: {model.ProvaAlunoId}");
            Console.WriteLine($"Respostas.Count: {model.Respostas?.Count ?? 0}");
            
            if (model.Respostas != null)
            {
                for (int i = 0; i < model.Respostas.Count; i++)
                {
                    var resposta = model.Respostas[i];
                    Console.WriteLine($"Resposta[{i}]: QuestaoId={resposta.QuestaoId}, AlternativaId={resposta.AlternativaId}");
                }
            }
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== DEBUG: ModelState inválido ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Erro: {error.ErrorMessage}");
                }
                
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = new SerializableError(ModelState)
                });
            }

            try
            {
                var provaAluno = _repository.Get<ProvasAlunosEntity>(x => x.ID == model.ProvaAlunoId)
                    .Include(x => x.Prova)
                    .FirstOrDefault();

                if (provaAluno == null)
                {
                    return NotFound(new ReturnApiData<ProvasAlunosEntity> { Success = false, Message = "Prova não encontrada" });
                }

                if (provaAluno.Concluida)
                {
                    return BadRequest(new ReturnApiData<object>
                    {
                        Success = false,
                        Message = "Prova já foi finalizada"
                    });
                }

                // Calcular pontuação
                int pontuacaoTotal = 0;
                Console.WriteLine($"=== DEBUG: Processando {model.Respostas?.Count ?? 0} respostas ===");
                
                foreach (var resposta in model.Respostas ?? new List<RespostaDto>())
                {
                    var alternativa = _repository.Get<AlternativasEntity>(x => x.ID == resposta.AlternativaId).FirstOrDefault();
                    var questao = _repository.Get<QuestoesEntity>(x => x.ID == resposta.QuestaoId).FirstOrDefault();
                    
                    Console.WriteLine($"Processando resposta: QuestaoId={resposta.QuestaoId}, AlternativaId={resposta.AlternativaId}");
                    Console.WriteLine($"  - Alternativa encontrada: {alternativa != null}");
                    Console.WriteLine($"  - Questão encontrada: {questao != null}");
                    Console.WriteLine($"  - Alternativa correta: {alternativa?.Correta}");
                    
                    if (alternativa != null && alternativa.Correta)
                    {
                        pontuacaoTotal += questao?.Pontos ?? 1;
                        Console.WriteLine($"  - Pontos adicionados: {questao?.Pontos ?? 1}");
                    }

                    // Salvar resposta do aluno
                    var respostaAluno = new RespostasAlunosEntity
                    {
                        ProvaAlunoId = provaAluno.ID,
                        QuestaoId = resposta.QuestaoId,
                        AlternativaId = resposta.AlternativaId,
                        Correta = alternativa?.Correta ?? false,
                        PontosObtidos = alternativa?.Correta == true ? (questao?.Pontos ?? 1) : 0
                    };

                    _repository.Add(respostaAluno);
                    Console.WriteLine($"  - Resposta salva no banco");
                }

                Console.WriteLine($"=== DEBUG: Pontuação total: {pontuacaoTotal} ===");

                // Atualizar prova do aluno
                provaAluno.DataFim = DateTime.Now;
                provaAluno.Pontuacao = pontuacaoTotal;
                provaAluno.Concluida = true;

                _repository.Edit(provaAluno);

                // Calcular se foi aprovado (70% ou mais)
                var totalQuestoes = _repository.Get<QuestoesEntity>(x => x.ATIVO && x.ProvaId == provaAluno.ProvaId).Count();
                var pontuacaoMaxima = _repository.Get<QuestoesEntity>(x => x.ATIVO && x.ProvaId == provaAluno.ProvaId).Sum(x => x.Pontos);
                var percentual = (double)pontuacaoTotal / pontuacaoMaxima * 100;
                provaAluno.Aprovada = percentual >= 70;

                // Gerar recomendações do ChatGPT
                var respostasErradas = await AnalisarRespostasErradas(provaAluno.ID, model.Respostas ?? new List<RespostaDto>());
                var nota = (double)pontuacaoTotal / pontuacaoMaxima * 10;
                var acertos = (model.Respostas ?? new List<RespostaDto>()).Count(r => _repository.Get<AlternativasEntity>(a => a.ID == r.AlternativaId).FirstOrDefault()?.Correta == true);
                
                provaAluno.RecomendacoesChatGPT = await _chatGPTService.GerarRecomendacoesAsync(
                    provaAluno.Prova.Titulo, 
                    nota, 
                    acertos, 
                    totalQuestoes, 
                    respostasErradas
                );

                return Ok(new ReturnApiData<ProvasAlunosEntity> { Success = true, Data = provaAluno });
            }
            catch (Exception e)
            {
                Console.WriteLine($"=== DEBUG: Exception: {e} ===");
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = e?.InnerException?.Message ?? e.Message
                });
            }
        }

        [HttpGet, Route("Prova/{provaId:guid}/Resultados"), Produces("application/json", Type = typeof(ReturnApiData<List<ResultadoAlunoDto>>))]
        public ObjectResult GetResultadosPorProva(Guid provaId)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            
            var resultados = _repository.Get<ProvasAlunosEntity>(x => x.ProvaId == provaId && x.Concluida)
                .Include(x => x.Aluno)
                .AsNoTracking()
                .ToList();

            var resultadosDto = resultados.Select(r => new ResultadoAlunoDto
            {
                ProvaAlunoId = r.ID,
                AlunoNome = r.Aluno.NOME,
                AlunoEmail = r.Aluno.EMAIL,
                Nota = CalcularNota(r.Pontuacao, provaId),
                Acertos = CalcularAcertos(r.ID),
                TempoGasto = CalcularTempoGasto(r.DataInicio, r.DataFim),
                RecomendacoesLiberadas = r.RecomendacoesLiberadas
            }).ToList();

            return Ok(new ReturnApiData<List<ResultadoAlunoDto>> { Success = true, Data = resultadosDto });
        }

        private double CalcularNota(int pontuacao, Guid provaId)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var pontuacaoMaxima = _repository.Get<QuestoesEntity>(x => x.ATIVO && x.ProvaId == provaId).Sum(x => x.Pontos);
            return pontuacaoMaxima > 0 ? (double)pontuacao / pontuacaoMaxima * 10 : 0;
        }

        private int CalcularAcertos(Guid provaAlunoId)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            return _repository.Get<RespostasAlunosEntity>(x => x.ProvaAlunoId == provaAlunoId && x.Correta).Count();
        }

        private int CalcularTempoGasto(DateTime dataInicio, DateTime? dataFim)
        {
            if (!dataFim.HasValue) return 0;
            var tempo = dataFim.Value - dataInicio;
            return (int)tempo.TotalMinutes;
        }

        private async Task<List<Project.BLL.Services.ChatGPTService.RespostaAnalise>> AnalisarRespostasErradas(Guid provaAlunoId, List<RespostaDto> respostas)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var respostasErradas = new List<Project.BLL.Services.ChatGPTService.RespostaAnalise>();

            foreach (var resposta in respostas)
            {
                var alternativa = _repository.Get<AlternativasEntity>(x => x.ID == resposta.AlternativaId).FirstOrDefault();
                var questao = _repository.Get<QuestoesEntity>(x => x.ID == resposta.QuestaoId).FirstOrDefault();
                
                if (alternativa != null && questao != null && !alternativa.Correta)
                {
                    // Buscar a alternativa correta
                    var alternativaCorreta = _repository.Get<AlternativasEntity>(x => x.QuestaoId == questao.ID && x.Correta).FirstOrDefault();
                    
                    respostasErradas.Add(new Project.BLL.Services.ChatGPTService.RespostaAnalise
                    {
                        NumeroQuestao = questao.Ordem,
                        TextoQuestao = questao.Enunciado,
                        RespostaEscolhida = alternativa.Texto,
                        RespostaCorreta = alternativaCorreta?.Texto ?? "Não disponível"
                    });
                }
            }

            return respostasErradas;
        }

        [HttpPatch, Route("{id:guid}/LiberarRecomendacoes"), Produces("application/json", Type = typeof(ReturnApiData<ProvasAlunosEntity>))]
        public ObjectResult LiberarRecomendacoes(Guid id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var provaAluno = _repository.Get<ProvasAlunosEntity>(x => x.ID == id).FirstOrDefault();

            if (provaAluno == null)
            {
                return NotFound(new ReturnApiData<ProvasAlunosEntity> { Success = false, Message = "Prova não encontrada" });
            }

            if (!provaAluno.Concluida)
            {
                return BadRequest(new ReturnApiData<object>
                {
                    Success = false,
                    Message = "A prova ainda não foi concluída"
                });
            }

            try
            {
                provaAluno.RecomendacoesLiberadas = true;
                _repository.Edit(provaAluno);
                return Ok(new ReturnApiData<ProvasAlunosEntity> { Success = true, Data = provaAluno });
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

        [HttpGet, Route("{id:guid}/Detalhes"), Produces("application/json", Type = typeof(ReturnApiData<List<DetalheQuestaoDto>>))]
        public ObjectResult GetDetalhesQuestoes(Guid id)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            
            var respostas = _repository.Get<RespostasAlunosEntity>(x => x.ProvaAlunoId == id)
                .Include(x => x.Questao)
                .Include(x => x.Alternativa)
                .AsNoTracking()
                .ToList();

            var detalhes = respostas.Select(r => new DetalheQuestaoDto
            {
                NumeroQuestao = r.Questao.Ordem,
                TextoQuestao = r.Questao.Enunciado,
                RespostaEscolhida = r.Alternativa.Texto,
                RespostaCorreta = BuscarRespostaCorreta(r.QuestaoId),
                Correta = r.Correta
            }).OrderBy(x => x.NumeroQuestao).ToList();

            return Ok(new ReturnApiData<List<DetalheQuestaoDto>> { Success = true, Data = detalhes });
        }

        private string BuscarRespostaCorreta(Guid questaoId)
        {
            using var _repository = new RepoDataBase(_configuration, _context);
            var alternativaCorreta = _repository.Get<AlternativasEntity>(x => x.QuestaoId == questaoId && x.Correta).FirstOrDefault();
            return alternativaCorreta?.Texto ?? "Não disponível";
        }

        public class ProvaAlunoDto
        {
            public Guid ID { get; set; }
            public Guid ProvaId { get; set; }
            public string ProvaTitulo { get; set; }
            public Guid AlunoId { get; set; }
            public string AlunoNome { get; set; }
            public DateTime DataInicio { get; set; }
            public DateTime? DataFim { get; set; }
            public int Pontuacao { get; set; }
            public bool Concluida { get; set; }
            public bool Aprovada { get; set; }
            public bool RecomendacoesLiberadas { get; set; }
        }

        public class IniciarProvaDto
        {
            public Guid ProvaId { get; set; }
            public Guid AlunoId { get; set; }
        }

        public class FinalizarProvaDto
        {
            public Guid ProvaAlunoId { get; set; }
            public List<RespostaDto> Respostas { get; set; } = new();
        }

        public class RespostaDto
        {
            public Guid QuestaoId { get; set; }
            public Guid AlternativaId { get; set; }
        }

        public class ResultadoAlunoDto
        {
            public Guid ProvaAlunoId { get; set; }
            public string AlunoNome { get; set; }
            public string AlunoEmail { get; set; }
            public double Nota { get; set; }
            public int Acertos { get; set; }
            public int TempoGasto { get; set; }
            public bool RecomendacoesLiberadas { get; set; }
        }

        public class DetalheQuestaoDto
        {
            public int NumeroQuestao { get; set; }
            public string TextoQuestao { get; set; } = string.Empty;
            public string RespostaEscolhida { get; set; } = string.Empty;
            public string RespostaCorreta { get; set; } = string.Empty;
            public bool Correta { get; set; }
        }
    }
} 