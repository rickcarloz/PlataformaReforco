using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace WebUI.Pages.Turmas
{
    public class FormModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        [BindProperty]
        public TurmaDto Turma { get; set; }
        public bool IsEdit { get; set; }
        public string ErrorMessage { get; set; }
        public List<ProfessorDto> Professores { get; set; } = new();

        public FormModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            await CarregarProfessores(token);
            if (id.HasValue)
            {
                IsEdit = true;
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                var response = await client.GetAsync($"v1/Turmas/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ReturnTurmaResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Turma = result?.Data;
                }
                else
                {
                    ErrorMessage = "Turma não encontrada.";
                }
            }
            else
            {
                Turma = new TurmaDto();
                IsEdit = false;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");
            // Definir ano corrente e professor logado
            Turma.Ano = DateTime.Now.Year;
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                ErrorMessage = "Não foi possível identificar o professor logado.";
                return Page();
            }
            Turma.UsuarioId = userId;
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var json = JsonSerializer.Serialize(Turma);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response;
            if (Turma.ID != Guid.Empty)
            {
                // PATCH para edição (apenas campos editáveis)
                var patchDoc = $"[{{\"op\":\"replace\",\"path\":\"/Disciplina\",\"value\":\"{Turma.Disciplina}\"}},{{\"op\":\"replace\",\"path\":\"/Serie\",\"value\":\"{Turma.Serie}\"}},{{\"op\":\"replace\",\"path\":\"/Turno\",\"value\":\"{Turma.Turno}\"}}]";
                content = new StringContent(patchDoc, Encoding.UTF8, "application/json-patch+json");
                response = await client.PatchAsync($"v1/Turmas/{Turma.ID}", content);
            }
            else
            {
                // POST para novo
                response = await client.PostAsync("v1/Turmas", content);
            }
            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Turmas/Index");
            }
            else
            {
                ErrorMessage = "Erro ao salvar turma.";
                return Page();
            }
        }

        private async Task CarregarProfessores(string token)
        {
            var client = _httpClientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            var response = await client.GetAsync("v1/Admin/Usuario");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaginateProfessorResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Professores = result?.Data ?? new List<ProfessorDto>();
            }
        }

        public class TurmaDto
        {
            public Guid ID { get; set; }
            public string Disciplina { get; set; }
            public string Serie { get; set; }
            public string Turno { get; set; }
            public int Ano { get; set; }
            public Guid UsuarioId { get; set; }
        }
        public class ReturnTurmaResult
        {
            public TurmaDto Data { get; set; }
        }
        public class ProfessorDto
        {
            public Guid ID { get; set; }
            public string Nome { get; set; }
        }
        public class PaginateProfessorResult
        {
            public List<ProfessorDto> Data { get; set; }
        }
    }
} 