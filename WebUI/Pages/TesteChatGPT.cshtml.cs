using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebUI.Pages
{
    public class TesteChatGPTModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string Recomendacoes { get; set; }

        public TesteChatGPTModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");
                
                // Para teste, não precisamos de token
                var response = await client.PostAsync("v1/ProvasAlunos/TesteChatGPT", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TesteChatGPTResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (result?.Success == true && result.Data != null)
                    {
                        SuccessMessage = "Teste realizado com sucesso!";
                        Recomendacoes = result.Data.Recomendacoes;
                    }
                    else
                    {
                        ErrorMessage = result?.Message?.ToString() ?? "Erro desconhecido";
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Erro HTTP {response.StatusCode}: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
            }

            return Page();
        }

        public class TesteChatGPTResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public TesteChatGPTData Data { get; set; }
        }

        public class TesteChatGPTData
        {
            public string Recomendacoes { get; set; }
            public string Mensagem { get; set; }
        }
    }
} 