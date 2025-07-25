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
    public class FormModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        [BindProperty]
        public AlunoEditDto Aluno { get; set; }
        public bool IsEdit { get; set; }
        public string ErrorMessage { get; set; }
        public List<TurmaDto> Turmas { get; set; } = new();

        public FormModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            
            await CarregarTurmas(token);
            
            if (id.HasValue)
            {
                IsEdit = true;
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                var response = await client.GetAsync($"v1/Admin/Alunos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ReturnAlunoResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Aluno = result?.Data;
                }
                else
                {
                    ErrorMessage = "Aluno não encontrado.";
                }
            }
            else
            {
                Aluno = new AlunoEditDto();
                IsEdit = false;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            
            await CarregarTurmas(token);
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            
            HttpResponseMessage response;
            if (Aluno.ID != Guid.Empty)
            {
                // PATCH para edição
                var patchDoc = $"[{{\"op\":\"replace\",\"path\":\"/NOME\",\"value\":\"{Aluno.NOME}\"}},{{\"op\":\"replace\",\"path\":\"/EMAIL\",\"value\":\"{Aluno.EMAIL}\"}},{{\"op\":\"replace\",\"path\":\"/USUARIO\",\"value\":\"{Aluno.USUARIO}\"}},{{\"op\":\"replace\",\"path\":\"/TurmaId\",\"value\":\"{Aluno.TurmaId}\"}}]";
                var content = new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json");
                response = await client.PatchAsync($"v1/Admin/Alunos/{Aluno.ID}", content);
            }
            else
            {
                // POST para novo
                var json = JsonSerializer.Serialize(Aluno);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await client.PostAsync("v1/Admin/Alunos", content);
            }
            
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Alunos/Index");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ErrorMessage = "Erro ao salvar aluno: " + error;
                return Page();
            }
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

        public class AlunoEditDto
        {
            public Guid ID { get; set; }
            public string NOME { get; set; }
            public string EMAIL { get; set; }
            public string USUARIO { get; set; }
            public Guid TurmaId { get; set; }
        }
        
        public class ReturnAlunoResult
        {
            public AlunoEditDto Data { get; set; }
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
    }
} 