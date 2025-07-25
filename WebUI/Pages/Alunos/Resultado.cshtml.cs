using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebUI.Pages.Alunos
{
    public class ResultadoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResultadoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public Guid ProvaAlunoId { get; set; }

        public ResultadoProvaDto? Resultado { get; set; }
        public List<DetalheQuestaoDto>? DetalhesQuestoes { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid provaAlunoId)
        {
            ProvaAlunoId = provaAlunoId;
            await CarregarResultado();
            return Page();
        }

        private async Task CarregarResultado()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");

                // Buscar resultado da prova
                var response = await client.GetAsync($"/v1/ProvasAlunos/{ProvaAlunoId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<ResultadoProvaDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Resultado = result?.Data;
                }

                // Buscar detalhes das questões
                var responseDetalhes = await client.GetAsync($"/v1/ProvasAlunos/{ProvaAlunoId}/Detalhes");
                if (responseDetalhes.IsSuccessStatusCode)
                {
                    var contentDetalhes = await responseDetalhes.Content.ReadAsStringAsync();
                    var resultDetalhes = JsonSerializer.Deserialize<ApiResponse<List<DetalheQuestaoDto>>>(contentDetalhes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    DetalhesQuestoes = resultDetalhes?.Data ?? new List<DetalheQuestaoDto>();
                }

                // MOCK: Se não houver dados reais, preencher com dados mocados para apresentação
                if (Resultado == null)
                {
                    Resultado = new ResultadoProvaDto
                    {
                        ProvaTitulo = "Prova de Matemática",
                        TurmaNome = "Turma A",
                        ProfessorNome = "Prof. Carlos",
                        AlunoNome = "João da Silva",
                        DataInicio = DateTime.Now.AddMinutes(-45),
                        DataFim = DateTime.Now,
                        Nota = 8.7,
                        Acertos = 13,
                        TotalQuestoes = 15,
                        TempoGasto = 45,
                        Aprovado = true,
                        RecomendacoesChatGPT = "- Revise os conceitos de equações do 2º grau\n- Pratique problemas de geometria\n- Assista à videoaula sobre funções\n- Consulte o material complementar no Google Classroom",
                        RecomendacoesLiberadas = true
                    };
                }
                if (DetalhesQuestoes == null || !DetalhesQuestoes.Any())
                {
                    DetalhesQuestoes = new List<DetalheQuestaoDto>
                    {
                        new DetalheQuestaoDto { NumeroQuestao = 1, TextoQuestao = "Qual o valor de x na equação x²-4=0?", RespostaEscolhida = "2", RespostaCorreta = "2", Correta = true },
                        new DetalheQuestaoDto { NumeroQuestao = 2, TextoQuestao = "Qual a área de um círculo de raio 3?", RespostaEscolhida = "28,27", RespostaCorreta = "28,27", Correta = true },
                        new DetalheQuestaoDto { NumeroQuestao = 3, TextoQuestao = "Resolva: 5x-10=0", RespostaEscolhida = "2", RespostaCorreta = "2", Correta = true },
                        new DetalheQuestaoDto { NumeroQuestao = 4, TextoQuestao = "O que é uma função do 1º grau?", RespostaEscolhida = "É uma função polinomial de grau 1", RespostaCorreta = "É uma função polinomial de grau 1", Correta = true },
                        new DetalheQuestaoDto { NumeroQuestao = 5, TextoQuestao = "Qual a fórmula de Bhaskara?", RespostaEscolhida = "x = (-b±√(b²-4ac))/2a", RespostaCorreta = "x = (-b±√(b²-4ac))/2a", Correta = true }
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar resultado: {ex.Message}";
            }
        }
    }

    public class ResultadoProvaDto
    {
        public Guid ID { get; set; }
        public string ProvaTitulo { get; set; } = string.Empty;
        public string TurmaNome { get; set; } = string.Empty;
        public string ProfessorNome { get; set; } = string.Empty;
        public string AlunoNome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public double Nota { get; set; }
        public int Acertos { get; set; }
        public int TotalQuestoes { get; set; }
        public int TempoGasto { get; set; }
        public bool Aprovado { get; set; }
        public string? RecomendacoesChatGPT { get; set; }
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

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
} 