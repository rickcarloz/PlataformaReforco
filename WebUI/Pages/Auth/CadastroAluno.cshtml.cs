using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace WebUI.Pages.Auth
{
    public class CadastroAlunoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        [BindProperty]
        public AlunoDto Aluno { get; set; } = new();
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public CadastroAlunoModel(IHttpClientFactory httpClientFactory)
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
            Aluno.Professor = false;
            var client = _httpClientFactory.CreateClient("API");
            var json = JsonSerializer.Serialize(Aluno);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("v1/Admin/Usuario", content);
            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Cadastro realizado com sucesso! Você receberá um e-mail para definir sua senha.";
                Aluno = new AlunoDto();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ErrorMessage = "Erro ao cadastrar: " + error;
            }
            return Page();
        }

        public class AlunoDto
        {
            public string USUARIO { get; set; }
            public string NOME { get; set; }
            public string EMAIL { get; set; }
            public bool Professor { get; set; }
        }
    }
} 