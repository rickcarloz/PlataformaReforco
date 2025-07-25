using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace WebUI.Pages.Turmas
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<TurmaDto> Turmas { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            var isProfessorStr = HttpContext.Session.GetString("IsProfessor");
            if (string.IsNullOrEmpty(isProfessorStr) || !bool.TryParse(isProfessorStr, out var isProfessor) || !isProfessor)
                return RedirectToPage("/Dashboard/Index");
            var usuarioIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(usuarioIdStr) || !Guid.TryParse(usuarioIdStr, out var usuarioId))
            {
                Turmas = new List<TurmaDto>();
                return Page();
            }
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync($"v1/Turmas?usuarioId={usuarioId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateTurmaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Turmas = result?.Data ?? new List<TurmaDto>();
            }
            else
            {
                Turmas = new List<TurmaDto>();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.DeleteAsync($"v1/Turmas/{id}");
            return RedirectToPage();
        }

        public class TurmaDto
        {
            public Guid ID { get; set; }
            public string Disciplina { get; set; }
            public string Serie { get; set; }
            public string Turno { get; set; }
            public int Ano { get; set; }
            public Guid UsuarioId { get; set; }
            public string ProfessorNome { get; set; }
            public int QuantidadeAlunos { get; set; }
        }
        public class PaginateTurmaResult
        {
            public List<TurmaDto> Data { get; set; }
        }
    }
} 