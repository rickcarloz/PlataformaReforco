using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebUI.Pages.Alunos
{
    public class FazerProvaModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FazerProvaModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ProvaCompletaDto Prova { get; set; }
        public Guid ProvaAlunoId { get; set; }
        public int TempoRestante { get; set; } = 0;
        public List<RespostaDto> Respostas { get; set; } = new();
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
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

                // Buscar prova
                var response = await client.GetAsync($"v1/Provas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ReturnProvaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (result?.Success == true && result.Data != null)
                    {
                        Prova = result.Data;
                        
                        // Verificar se já iniciou a prova
                        var alunoId = await GetAlunoId(token);
                        if (alunoId != Guid.Empty)
                        {
                            var provasAlunoResponse = await client.GetAsync($"v1/ProvasAlunos/Aluno/{alunoId}");
                            if (provasAlunoResponse.IsSuccessStatusCode)
                            {
                                var provasAlunoJson = await provasAlunoResponse.Content.ReadAsStringAsync();
                                var provasAlunoResult = JsonSerializer.Deserialize<PaginateProvaAlunoResult>(provasAlunoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                
                                var provaAluno = provasAlunoResult?.Data?.FirstOrDefault(pa => pa.ProvaId == id);
                                if (provaAluno != null)
                                {
                                    ProvaAlunoId = provaAluno.ID;
                                    
                                    if (provaAluno.Concluida)
                                    {
                                        return RedirectToPage("/Alunos/Resultado", new { id = provaAluno.ID });
                                    }
                                    
                                    // Calcular tempo restante
                                    var tempoDecorrido = (DateTime.Now - provaAluno.DataInicio).TotalMinutes;
                                    TempoRestante = Math.Max(0, Prova.TempoLimite - (int)tempoDecorrido);
                                }
                                else
                                {
                                    // Iniciar nova prova
                                    var iniciouComSucesso = await IniciarProva(client, id, alunoId);
                                    if (!iniciouComSucesso)
                                    {
                                        ErrorMessage = "Erro ao iniciar prova.";
                                        return Page();
                                    }
                                    TempoRestante = Prova.TempoLimite;
                                }
                            }
                            else
                            {
                                // Se não conseguir buscar provas do aluno, iniciar nova prova
                                var iniciouComSucesso = await IniciarProva(client, id, alunoId);
                                if (!iniciouComSucesso)
                                {
                                    ErrorMessage = "Erro ao iniciar prova.";
                                    return Page();
                                }
                                TempoRestante = Prova.TempoLimite;
                            }
                        }
                        else
                        {
                            ErrorMessage = "Não foi possível identificar o aluno.";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Prova não encontrada.";
                    }
                }
                else
                {
                    ErrorMessage = "Erro ao carregar prova.";
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

            // Ler ProvaAlunoId do formulário
            var provaAlunoIdString = Request.Form["ProvaAlunoId"].ToString();
            if (!Guid.TryParse(provaAlunoIdString, out Guid provaAlunoId) || provaAlunoId == Guid.Empty)
            {
                ErrorMessage = "Prova não foi iniciada corretamente.";
                return Page();
            }

            ProvaAlunoId = provaAlunoId;

            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

                // Processar respostas do formulário
                var formData = Request.Form;
                var respostas = new List<RespostaDto>();
                
                // Debug: Logar todos os campos do formulário
                Console.WriteLine("=== DEBUG: Campos do Formulário ===");
                foreach (var field in formData)
                {
                    Console.WriteLine($"{field.Key}: {field.Value}");
                }
                
                // Buscar todas as questões da prova
                if (Prova?.Questoes != null)
                {
                    Console.WriteLine($"=== DEBUG: Processando {Prova.Questoes.Count} questões ===");
                    
                    for (int i = 0; i < Prova.Questoes.Count; i++)
                    {
                        var questaoIdKey = $"Respostas[{i}].QuestaoId";
                        var alternativaIdKey = $"Respostas[{i}].AlternativaId";
                        
                        var questaoId = formData[questaoIdKey].ToString();
                        var alternativaId = formData[alternativaIdKey].ToString();
                        
                        Console.WriteLine($"Questão {i}: QuestaoId='{questaoId}', AlternativaId='{alternativaId}'");
                        
                        if (Guid.TryParse(questaoId, out Guid questaoGuid) && 
                            Guid.TryParse(alternativaId, out Guid alternativaGuid))
                        {
                            respostas.Add(new RespostaDto
                            {
                                QuestaoId = questaoGuid,
                                AlternativaId = alternativaGuid
                            });
                            Console.WriteLine($"✓ Questão {i} adicionada: {questaoGuid} -> {alternativaGuid}");
                        }
                        else
                        {
                            Console.WriteLine($"✗ Questão {i} inválida: QuestaoId={questaoId}, AlternativaId={alternativaId}");
                        }
                    }
                }

                Console.WriteLine($"=== DEBUG: Total de respostas coletadas: {respostas.Count} ===");

                // Finalizar prova
                var finalizarDto = new FinalizarProvaDto
                {
                    ProvaAlunoId = ProvaAlunoId,
                    Respostas = respostas
                };

                var json = JsonSerializer.Serialize(finalizarDto);
                Console.WriteLine($"=== DEBUG: JSON enviado: {json} ===");
                
                // Debug: Verificar se o JSON pode ser deserializado corretamente
                try
                {
                    var testDeserialize = JsonSerializer.Deserialize<FinalizarProvaDto>(json);
                    Console.WriteLine($"=== DEBUG: Deserialização teste - ProvaAlunoId: {testDeserialize.ProvaAlunoId}, Respostas.Count: {testDeserialize.Respostas?.Count ?? 0} ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"=== DEBUG: Erro na deserialização teste: {ex.Message} ===");
                }
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/ProvasAlunos/Finalizar", content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("/Alunos/Resultado", new { id = ProvaAlunoId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erro ao finalizar prova: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
                Console.WriteLine($"=== DEBUG: Exception: {ex} ===");
            }

            return Page();
        }

        private async Task<bool> IniciarProva(HttpClient client, Guid provaId, Guid alunoId)
        {
            try
            {
                var iniciarDto = new IniciarProvaDto
                {
                    ProvaId = provaId,
                    AlunoId = alunoId
                };

                var json = JsonSerializer.Serialize(iniciarDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("v1/ProvasAlunos/Iniciar", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ReturnProvaAlunoResult>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result?.Success == true && result.Data != null)
                    {
                        ProvaAlunoId = result.Data.ID;
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
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

        public class ProvaCompletaDto
        {
            public Guid ID { get; set; }
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public int TempoLimite { get; set; }
            public List<QuestaoDto> Questoes { get; set; } = new();
        }

        public class QuestaoDto
        {
            public Guid ID { get; set; }
            public string Enunciado { get; set; }
            public int Pontos { get; set; }
            public List<AlternativaDto> Alternativas { get; set; } = new();
        }

        public class AlternativaDto
        {
            public Guid ID { get; set; }
            public string Texto { get; set; }
            public string Letra { get; set; }
        }

        public class RespostaDto
        {
            public Guid QuestaoId { get; set; }
            public Guid AlternativaId { get; set; }
        }

        public class ReturnProvaResult
        {
            public bool Success { get; set; }
            public ProvaCompletaDto Data { get; set; }
        }

        public class PaginateProvaAlunoResult
        {
            public List<ProvaAlunoInfoDto> Data { get; set; } = new();
        }

        public class ProvaAlunoInfoDto
        {
            public Guid ID { get; set; }
            public Guid ProvaId { get; set; }
            public DateTime DataInicio { get; set; }
            public bool Concluida { get; set; }
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

        public class ReturnProvaAlunoResult
        {
            public bool Success { get; set; }
            public ProvaAlunoDto Data { get; set; }
        }

        public class ProvaAlunoDto
        {
            public Guid ID { get; set; }
            public Guid ProvaId { get; set; }
            public Guid AlunoId { get; set; }
            public DateTime DataInicio { get; set; }
        }
    }
} 