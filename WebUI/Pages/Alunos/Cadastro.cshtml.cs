using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebUI.Pages.Alunos
{
    public class CadastroModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        [BindProperty]
        public AlunoCadastroDto Aluno { get; set; } = new();
        public List<TurmaDto> Turmas { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public CadastroModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            
            await CarregarTurmas(token);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            await CarregarTurmas(token);
            Aluno.ProfessorId = await GetProfessorId(token);
            if (Aluno.ProfessorId == Guid.Empty)
            {
                ErrorMessage = "Não foi possível identificar o professor. Verifique se você está logado como professor.";
                return Page();
            }
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var json = JsonSerializer.Serialize(Aluno);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("v1/Admin/Alunos", content);
            if (response.IsSuccessStatusCode)
            {
                // Após sucesso, redirecionar para a listagem de alunos
                return RedirectToPage("/Alunos/Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ErrorMessage = "Erro ao cadastrar: " + error;
            }
            return Page();
        }

        private async Task CarregarTurmas(string token)
        {
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync("v1/Turmas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateTurmaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Turmas = result?.Data ?? new List<TurmaDto>();
            }
        }

        private async Task<Guid> GetProfessorId(string token)
        {
            try
            {
                // Primeiro, tentar obter o ID do usuário da sessão
                var userIdString = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out Guid userId))
                {
                    // Verificar se o usuário é professor
                    var isProfessorString = HttpContext.Session.GetString("IsProfessor");
                    if (!string.IsNullOrEmpty(isProfessorString) && bool.TryParse(isProfessorString, out bool isProfessor) && isProfessor)
                    {
                        return userId;
                    }
                }
                
                // Se não conseguiu da sessão, tentar buscar da API
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                
                var response = await client.GetAsync("v1/Admin/Usuario");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginateUsuarioResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    // Se temos dados, vamos pegar o primeiro usuário que seja professor
                    if (result?.Data != null && result.Data.Any())
                    {
                        var professor = result.Data.FirstOrDefault(u => u.Professor);
                        if (professor != null)
                        {
                            return professor.ID;
                        }
                    }
                }
                
                ErrorMessage = "Não foi possível identificar o professor. Verifique se você está logado como professor.";
                return Guid.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao obter ID do professor: {ex.Message}";
                return Guid.Empty;
            }
        }

        public class AlunoCadastroDto
        {
            public string USUARIO { get; set; }
            public string NOME { get; set; }
            public string EMAIL { get; set; }
            public Guid ProfessorId { get; set; }
            public Guid TurmaId { get; set; }
        }
        
        public class TurmaDto
        {
            public Guid ID { get; set; }
            public string Serie { get; set; }
        }
        
        public class PaginateTurmaResult
        {
            public List<TurmaDto> Data { get; set; }
        }
        
        public class UsuarioDto
        {
            public Guid ID { get; set; }
            public string USUARIO { get; set; }
            public bool Professor { get; set; }
        }
        
        public class PaginateUsuarioResult
        {
            public List<UsuarioDto> Data { get; set; }
        }
    }
} 