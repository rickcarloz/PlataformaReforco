using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace WebUI.Pages.Auth
{
    public class CadastroProfessorModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        [BindProperty]
        public ProfessorDto Professor { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public CadastroProfessorModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ErrorMessage = null;
            SuccessMessage = null;
            Professor.Professor = true;
            var client = _httpClientFactory.CreateClient("API");
            var json = JsonSerializer.Serialize(Professor);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("v1/Admin/Usuario", content);
            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Cadastro realizado com sucesso! Você receberá um e-mail para definir sua senha.";
                Professor = new ProfessorDto();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ErrorMessage = "Erro ao cadastrar: " + error;
            }
            return Page();
        }

        public class ProfessorDto
        {
            public string USUARIO { get; set; }
            public string NOME { get; set; }
            public string EMAIL { get; set; }
            public bool Professor { get; set; }
        }
    }
} 