using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace WebUI.Pages.Alunos
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<AlunoDto> Alunos { get; set; }

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
                Alunos = new List<AlunoDto>();
                return Page();
            }
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync($"v1/Admin/Alunos?usuarioId={usuarioId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateAlunoResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Alunos = result?.Data ?? new List<AlunoDto>();
            }
            else
            {
                Alunos = new List<AlunoDto>();
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
            var response = await client.DeleteAsync($"v1/Admin/Alunos/{id}");
            return RedirectToPage();
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
        
        public class PaginateAlunoResult
        {
            public List<AlunoDto> Data { get; set; }
        }
    }
} 