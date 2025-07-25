using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebUI.Pages.Provas
{
    public class FormModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FormModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public ProvaDto Prova { get; set; } = new();

        [BindProperty]
        public List<QuestaoDto> Questoes { get; set; } = new();

        public List<TurmaDto> Turmas { get; set; } = new();
        public bool IsEdit { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                // Carregar turmas
                await CarregarTurmas(client);

                if (id.HasValue)
                {
                    IsEdit = true;
                    await CarregarProva(client, id.Value);
                }
                else
                {
                    IsEdit = false;
                    // Inicializar com uma questão vazia
                    Questoes.Add(new QuestaoDto
                    {
                        Alternativas = new List<AlternativaDto>
                        {
                            new AlternativaDto { Letra = "A", Texto = "", Correta = true },
                            new AlternativaDto { Letra = "B", Texto = "", Correta = false }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                // Validar questões
                if (!Questoes.Any())
                {
                    ErrorMessage = "Adicione pelo menos uma questão.";
                    await CarregarTurmas(client);
                    return Page();
                }

                if (Questoes.Count > 10)
                {
                    ErrorMessage = "Máximo de 10 questões permitido.";
                    await CarregarTurmas(client);
                    return Page();
                }

                // Processar alternativas corretas do formulário
                var formData = Request.Form;
                for (int i = 0; i < Questoes.Count; i++)
                {
                    var alternativaCorretaIndex = formData[$"Questoes[{i}].AlternativaCorreta"].ToString();
                    if (int.TryParse(alternativaCorretaIndex, out int indexCorreta))
                    {
                        // Marcar todas como falsas primeiro
                        for (int j = 0; j < Questoes[i].Alternativas.Count; j++)
                        {
                            Questoes[i].Alternativas[j].Correta = false;
                        }
                        // Marcar a selecionada como correta
                        if (indexCorreta >= 0 && indexCorreta < Questoes[i].Alternativas.Count)
                        {
                            Questoes[i].Alternativas[indexCorreta].Correta = true;
                        }
                    }
                }

                // Validar alternativas
                foreach (var questao in Questoes)
                {
                    if (questao.Alternativas.Count < 2)
                    {
                        ErrorMessage = "Cada questão deve ter pelo menos 2 alternativas.";
                        await CarregarTurmas(client);
                        return Page();
                    }

                    if (!questao.Alternativas.Any(a => a.Correta))
                    {
                        ErrorMessage = "Cada questão deve ter uma alternativa correta.";
                        await CarregarTurmas(client);
                        return Page();
                    }
                }

                // Preparar dados para envio
                var criarProvaDto = new CriarProvaDto
                {
                    Titulo = Prova.Titulo,
                    Descricao = Prova.Descricao,
                    TurmaId = Prova.TurmaId,
                    ProfessorId = await GetProfessorId(token),
                    TempoLimite = Prova.TempoLimite,
                    Questoes = Questoes.Select(q => new CriarQuestaoDto
                    {
                        Enunciado = q.Enunciado,
                        Pontos = q.Pontos,
                        Alternativas = q.Alternativas.Select(a => new CriarAlternativaDto
                        {
                            Texto = a.Texto,
                            Letra = a.Letra,
                            Correta = a.Correta
                        }).ToList()
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(criarProvaDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (IsEdit)
                {
                    response = await client.PatchAsync($"v1/Provas/{Prova.ID}", content);
                }
                else
                {
                    response = await client.PostAsync("v1/Provas", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = IsEdit ? "Prova atualizada com sucesso!" : "Prova criada com sucesso!";
                    return RedirectToPage("/Provas/Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erro ao salvar prova: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
            }

            await CarregarTurmas(_httpClientFactory.CreateClient("API"));
            return Page();
        }

        private async Task CarregarTurmas(HttpClient client)
        {
            var response = await client.GetAsync("v1/Turmas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateTurmaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Turmas = result?.Data ?? new();
            }
        }

        private async Task CarregarProva(HttpClient client, Guid id)
        {
            var response = await client.GetAsync($"v1/Provas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReturnProvaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result?.Success == true && result.Data != null)
                {
                    Prova = new ProvaDto
                    {
                        ID = result.Data.ID,
                        Titulo = result.Data.Titulo,
                        Descricao = result.Data.Descricao,
                        TurmaId = result.Data.TurmaId,
                        TempoLimite = result.Data.TempoLimite,
                        Ativa = result.Data.Ativa
                    };

                    Questoes = result.Data.Questoes.Select(q => new QuestaoDto
                    {
                        ID = q.ID,
                        Enunciado = q.Enunciado,
                        Pontos = q.Pontos,
                        Alternativas = q.Alternativas.Select(a => new AlternativaDto
                        {
                            ID = a.ID,
                            Texto = a.Texto,
                            Letra = a.Letra,
                            Correta = a.Correta
                        }).ToList()
                    }).ToList();
                }
            }
        }

        private async Task<Guid> GetProfessorId(string token)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
            {
                return userId;
            }

            // Fallback para API
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            
            var response = await client.GetAsync("v1/Admin/Usuario");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateUsuarioResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var professor = result?.Data?.FirstOrDefault(u => u.Professor);
                return professor?.ID ?? Guid.Empty;
            }

            return Guid.Empty;
        }

        public class ProvaDto
        {
            public Guid ID { get; set; }
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Guid TurmaId { get; set; }
            public int TempoLimite { get; set; }
            public bool Ativa { get; set; } = true;
        }

        public class QuestaoDto
        {
            public Guid ID { get; set; }
            public string Enunciado { get; set; }
            public int Pontos { get; set; } = 1;
            public List<AlternativaDto> Alternativas { get; set; } = new();
        }

        public class AlternativaDto
        {
            public Guid ID { get; set; }
            public string Texto { get; set; }
            public string Letra { get; set; }
            public bool Correta { get; set; }
        }

        public class TurmaDto
        {
            public Guid ID { get; set; }
            public string Serie { get; set; }
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
            public string Texto { get; set; }
            public string Letra { get; set; }
            public bool Correta { get; set; }
        }

        public class PaginateTurmaResult
        {
            public List<TurmaDto> Data { get; set; } = new();
        }

        public class ReturnProvaResult
        {
            public bool Success { get; set; }
            public ProvaCompletaDto Data { get; set; }
        }

        public class ProvaCompletaDto
        {
            public Guid ID { get; set; }
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public Guid TurmaId { get; set; }
            public int TempoLimite { get; set; }
            public bool Ativa { get; set; }
            public List<QuestaoCompletaDto> Questoes { get; set; } = new();
        }

        public class QuestaoCompletaDto
        {
            public Guid ID { get; set; }
            public string Enunciado { get; set; }
            public int Pontos { get; set; }
            public List<AlternativaCompletaDto> Alternativas { get; set; } = new();
        }

        public class AlternativaCompletaDto
        {
            public Guid ID { get; set; }
            public string Texto { get; set; }
            public string Letra { get; set; }
            public bool Correta { get; set; }
        }

        public class PaginateUsuarioResult
        {
            public List<UsuarioDto> Data { get; set; } = new();
        }

        public class UsuarioDto
        {
            public Guid ID { get; set; }
            public string NOME { get; set; }
            public bool Professor { get; set; }
        }
    }
} 