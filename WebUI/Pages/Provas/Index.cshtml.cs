using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebUI.Pages.Provas
{
    public class IndexModel : Microsoft.AspNetCore.Mvc.RazorPages.PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ProvaDto> Provas { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            var isProfessorStr = HttpContext.Session.GetString("IsProfessor");
            if (string.IsNullOrEmpty(isProfessorStr) || !bool.TryParse(isProfessorStr, out var isProfessor) || !isProfessor)
                return RedirectToPage("/Dashboard/Index");
            var professorIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(professorIdStr) || !Guid.TryParse(professorIdStr, out var professorId))
            {
                // Não conseguiu identificar o professor, retorna lista vazia
                Provas = new List<ProvaDto>();
                return Page();
            }
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync($"v1/Provas?professorId={professorId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateProvaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Provas = result?.Data ?? new List<ProvaDto>();
            }
            else
            {
                Provas = new List<ProvaDto>();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
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

                var response = await client.DeleteAsync($"v1/Provas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Prova excluída com sucesso!";
                }
                else
                {
                    ErrorMessage = "Erro ao excluir prova.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro: {ex.Message}";
            }

            return RedirectToPage();
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

        public class PaginateProvaResult
        {
            public List<ProvaDto> Data { get; set; } = new();
            public int Total { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
    }
} 