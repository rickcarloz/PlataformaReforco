using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebUI.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public string ErrorMessage { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string username, string password)
        {
            var client = _httpClientFactory.CreateClient("API");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserName", username),
                new KeyValuePair<string, string>("Password", password)
            });

            var response = await client.PostAsync("v1/Auth/Conta/Login", content);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                HttpContext.Session.SetString("Token", token);

                // Buscar informações do usuário logado
                await GetUserInfo(token, username);

                return RedirectToPage("/Dashboard/Index");
            }

            ErrorMessage = "Usuário ou senha inválidos.";
            return Page();
        }

        private async Task GetUserInfo(string token, string username)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
                
                var response = await client.GetAsync("v1/Admin/Usuario");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PaginateUsuarioResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    var user = result?.Data?.FirstOrDefault(u => u.USUARIO == username);
                    if (user != null)
                    {
                        HttpContext.Session.SetString("UserId", user.ID.ToString());
                        HttpContext.Session.SetString("UserName", user.USUARIO);
                        HttpContext.Session.SetString("IsProfessor", user.Professor.ToString());
                    }
                }
            }
            catch
            {
                // Se falhar, não impede o login
            }
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
