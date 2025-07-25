using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace WebUI.Pages.Turmas
{
    public class AlunosModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<AlunoDto> Alunos { get; set; } = new();
        public string TurmaNome { get; set; }
        public Guid TurmaId { get; set; }

        public AlunosModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            
            TurmaId = id;
            
            // Buscar informações da turma
            await CarregarTurma(token, id);
            
            // Buscar alunos da turma
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync($"v1/Admin/Alunos/Turma/{id}");
            
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
            return RedirectToPage(new { id = TurmaId });
        }

        private async Task CarregarTurma(string token, Guid turmaId)
        {
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync($"v1/Turmas/{turmaId}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReturnTurmaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TurmaNome = result?.Data?.Serie ?? "Turma não encontrada";
            }
            else
            {
                TurmaNome = "Turma não encontrada";
            }
        }

        public class AlunoDto
        {
            public Guid ID { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Usuario { get; set; }
            public string ProfessorNome { get; set; }
        }
        
        public class PaginateAlunoResult
        {
            public List<AlunoDto> Data { get; set; }
        }
        
        public class TurmaDto
        {
            public Guid ID { get; set; }
            public string Serie { get; set; }
        }
        
        public class ReturnTurmaResult
        {
            public TurmaDto Data { get; set; }
        }
    }
} 