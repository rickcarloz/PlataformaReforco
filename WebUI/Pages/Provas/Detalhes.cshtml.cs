using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace WebUI.Pages.Provas
{
    public class DetalhesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public DetalhesModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public Guid Id { get; set; }

        public ProvaDetalhesDto? Prova { get; set; }
        public List<QuestaoDto>? Questoes { get; set; }
        public List<ResultadoAlunoDto>? ResultadosAlunos { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        // Estatísticas calculadas
        public double? MediaGeral { get; set; }
        public double? MaiorNota { get; set; }
        public double? MenorNota { get; set; }
        public int TaxaAprovacao { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Id = id;
            await CarregarDadosProva();
            return Page();
        }

        public async Task<IActionResult> OnPostLiberarRecomendacoesAsync(Guid provaAlunoId)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.PatchAsync($"v1/ProvasAlunos/{provaAlunoId}/LiberarRecomendacoes", null);
            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Recomendações liberadas com sucesso!";
            }
            else
            {
                ErrorMessage = "Erro ao liberar recomendações. Tente novamente.";
            }
            await CarregarDadosProva();
            return Page();
        }

        private async Task CarregarDadosProva()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Buscar detalhes da prova
                var responseProva = await client.GetAsync($"/v1/Provas/{Id}/Detalhes");
                if (responseProva.IsSuccessStatusCode)
                {
                    var contentProva = await responseProva.Content.ReadAsStringAsync();
                    var resultProva = JsonSerializer.Deserialize<ApiResponse<ProvaDetalhesDto>>(contentProva, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Prova = resultProva?.Data;
                }

                // Buscar questões da prova
                var responseQuestoes = await client.GetAsync($"/v1/Provas/{Id}/Questoes");
                if (responseQuestoes.IsSuccessStatusCode)
                {
                    var contentQuestoes = await responseQuestoes.Content.ReadAsStringAsync();
                    var resultQuestoes = JsonSerializer.Deserialize<ApiResponse<List<QuestaoDto>>>(contentQuestoes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Questoes = resultQuestoes?.Data ?? new List<QuestaoDto>();
                }

                // Buscar resultados dos alunos
                var responseResultados = await client.GetAsync($"/v1/ProvasAlunos/Prova/{Id}/Resultados");
                if (responseResultados.IsSuccessStatusCode)
                {
                    var contentResultados = await responseResultados.Content.ReadAsStringAsync();
                    var resultResultados = JsonSerializer.Deserialize<ApiResponse<List<ResultadoAlunoDto>>>(contentResultados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ResultadosAlunos = resultResultados?.Data ?? new List<ResultadoAlunoDto>();
                }

                // Calcular estatísticas
                CalcularEstatisticas();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar dados: {ex.Message}";
            }
        }

        private void CalcularEstatisticas()
        {
            if (ResultadosAlunos == null || !ResultadosAlunos.Any())
            {
                MediaGeral = null;
                MaiorNota = null;
                MenorNota = null;
                TaxaAprovacao = 0;
                return;
            }

            var notas = ResultadosAlunos.Select(r => r.Nota).ToList();
            MediaGeral = notas.Average();
            MaiorNota = notas.Max();
            MenorNota = notas.Min();
            
            var aprovados = ResultadosAlunos.Count(r => r.Nota >= 7);
            TaxaAprovacao = (int)((double)aprovados / ResultadosAlunos.Count * 100);
        }
    }

    public class ProvaDetalhesDto
    {
        public Guid ID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string TurmaNome { get; set; } = string.Empty;
        public string ProfessorNome { get; set; } = string.Empty;
        public int QuantidadeQuestoes { get; set; }
        public int TempoLimite { get; set; }
        public bool Ativa { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class QuestaoDto
    {
        public Guid ID { get; set; }
        public string Enunciado { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public int Pontos { get; set; }
        public List<AlternativaDto>? Alternativas { get; set; }
    }

    public class AlternativaDto
    {
        public Guid ID { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string Letra { get; set; } = string.Empty;
        public bool Correta { get; set; }
    }

    public class ResultadoAlunoDto
    {
        public Guid ProvaAlunoId { get; set; }
        public string AlunoNome { get; set; } = string.Empty;
        public string AlunoEmail { get; set; } = string.Empty;
        public double Nota { get; set; }
        public int Acertos { get; set; }
        public int TempoGasto { get; set; }
        public bool RecomendacoesLiberadas { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
} 