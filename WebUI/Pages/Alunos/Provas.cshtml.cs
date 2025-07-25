using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebUI.Pages.Alunos
{
    public class ProvasModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProvasModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ProvaAlunoDto> ProvasPendentes { get; set; } = new();
        public List<ProvaAlunoDto> ProvasAguardandoAvaliacao { get; set; } = new();
        public List<ProvaAlunoDto> ProvasAvaliadas { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public int PaginaPendentes { get; set; } = 1;
        public int PaginaAguardando { get; set; } = 1;
        public int PaginaAvaliadas { get; set; } = 1;
        public int TotalPaginasPendentes { get; set; }
        public int TotalPaginasAguardando { get; set; }
        public int TotalPaginasAvaliadas { get; set; }
        private const int TamanhoPagina = 10;

        public async Task<IActionResult> OnGetAsync(int? pendentes, int? aguardando, int? avaliadas)
        {
            PaginaPendentes = pendentes ?? 1;
            PaginaAguardando = aguardando ?? 1;
            PaginaAvaliadas = avaliadas ?? 1;

            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            var isProfessorStr = HttpContext.Session.GetString("IsProfessor");
            if (string.IsNullOrEmpty(isProfessorStr) || (bool.TryParse(isProfessorStr, out var isProfessor) && isProfessor))
                return RedirectToPage("/Dashboard/Index");

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                // Buscar o aluno logado
                var alunoId = await GetAlunoId(token);
                if (alunoId == Guid.Empty)
                {
                    ErrorMessage = "Não foi possível identificar o aluno.";
                    return Page();
                }

                // Buscar dados do aluno para saber sua turma
                var alunoResponse = await client.GetAsync($"v1/Admin/Alunos");
                if (alunoResponse.IsSuccessStatusCode)
                {
                    var alunoJson = await alunoResponse.Content.ReadAsStringAsync();
                    var alunoResult = JsonSerializer.Deserialize<PaginateAlunoResult>(alunoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var aluno = alunoResult?.Data?.FirstOrDefault(a => a.ID == alunoId);
                    
                    if (aluno == null)
                    {
                        ErrorMessage = "Aluno não encontrado.";
                        return Page();
                    }

                    // Buscar provas da turma do aluno
                    var provasResponse = await client.GetAsync("v1/Provas");
                    if (provasResponse.IsSuccessStatusCode)
                    {
                        var provasJson = await provasResponse.Content.ReadAsStringAsync();
                        var provasResult = JsonSerializer.Deserialize<PaginateProvaResult>(provasJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var provasDaTurma = provasResult?.Data?.Where(p => p.TurmaId == aluno.TurmaId).ToList() ?? new();

                        // Buscar provas do aluno
                        var provasAlunoResponse = await client.GetAsync($"v1/ProvasAlunos/Aluno/{alunoId}");
                        var provasAluno = new List<ProvaAlunoDto>();
                        
                        if (provasAlunoResponse.IsSuccessStatusCode)
                        {
                            var provasAlunoJson = await provasAlunoResponse.Content.ReadAsStringAsync();
                            var provasAlunoResult = JsonSerializer.Deserialize<PaginateProvaAlunoResult>(provasAlunoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            provasAluno = provasAlunoResult?.Data ?? new();
                        }

                        // Combinar informações
                        var todasProvas = provasDaTurma.Select(p => {
                            var provaAluno = provasAluno.FirstOrDefault(pa => pa.ProvaId == p.ID);
                            return new ProvaAlunoDto
                            {
                                ID = p.ID,
                                Titulo = p.Titulo,
                                TurmaNome = p.TurmaNome,
                                ProfessorNome = p.ProfessorNome,
                                QuantidadeQuestoes = p.QuantidadeQuestoes,
                                TempoLimite = p.TempoLimite,
                                JaIniciada = provaAluno != null,
                                Concluida = provaAluno?.Concluida ?? false,
                                ProvaAlunoId = provaAluno?.ID ?? Guid.Empty,
                                RecomendacoesLiberadas = provaAluno?.RecomendacoesLiberadas ?? false,
                                ProvaId = p.ID
                            };
                        }).ToList();

                        ProvasPendentes = todasProvas.Where(p => !p.JaIniciada).ToList();
                        ProvasAguardandoAvaliacao = todasProvas.Where(p => p.JaIniciada && p.Concluida && !p.RecomendacoesLiberadas).ToList();
                        ProvasAvaliadas = todasProvas.Where(p => p.JaIniciada && p.Concluida && p.RecomendacoesLiberadas && p.ProvaAlunoId != Guid.Empty).ToList();

                        TotalPaginasPendentes = (int)Math.Ceiling((double)ProvasPendentes.Count / TamanhoPagina);
                        TotalPaginasAguardando = (int)Math.Ceiling((double)ProvasAguardandoAvaliacao.Count / TamanhoPagina);
                        TotalPaginasAvaliadas = (int)Math.Ceiling((double)ProvasAvaliadas.Count / TamanhoPagina);

                        ProvasPendentes = ProvasPendentes.Skip((PaginaPendentes - 1) * TamanhoPagina).Take(TamanhoPagina).ToList();
                        ProvasAguardandoAvaliacao = ProvasAguardandoAvaliacao.Skip((PaginaAguardando - 1) * TamanhoPagina).Take(TamanhoPagina).ToList();
                        ProvasAvaliadas = ProvasAvaliadas.Skip((PaginaAvaliadas - 1) * TamanhoPagina).Take(TamanhoPagina).ToList();
                    }
                    else
                    {
                        ErrorMessage = "Erro ao carregar provas.";
                    }
                }
                else
                {
                    ErrorMessage = "Erro ao carregar dados do aluno.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
            }

            return Page();
        }

        private async Task<Guid> GetAlunoId(string token)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
            {
                return userId;
            }

            return Guid.Empty;
        }

        public class ProvaAlunoDto
        {
            public Guid ID { get; set; }
            public Guid ProvaId { get; set; }
            public string Titulo { get; set; }
            public string TurmaNome { get; set; }
            public string ProfessorNome { get; set; }
            public int QuantidadeQuestoes { get; set; }
            public int TempoLimite { get; set; }
            public bool JaIniciada { get; set; }
            public bool Concluida { get; set; }
            public Guid ProvaAlunoId { get; set; }
            public bool RecomendacoesLiberadas { get; set; }
        }

        public class PaginateProvaResult
        {
            public List<ProvaDto> Data { get; set; } = new();
        }

        public class ProvaDto
        {
            public Guid ID { get; set; }
            public Guid TurmaId { get; set; }
            public string Titulo { get; set; }
            public string TurmaNome { get; set; }
            public string ProfessorNome { get; set; }
            public int QuantidadeQuestoes { get; set; }
            public int TempoLimite { get; set; }
        }

        public class PaginateProvaAlunoResult
        {
            public List<ProvaAlunoDto> Data { get; set; } = new();
        }

        public class PaginateAlunoResult
        {
            public List<AlunoDto> Data { get; set; } = new();
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