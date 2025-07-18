using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlataformaReforco.Data;
using PlataformaReforco.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PlataformaReforco.Controllers
{
    public class ConviteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ConviteController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Convite/Aceitar?token=...&turmaId=...
        [HttpGet]
        public IActionResult Aceitar(string token, Guid turmaId)
        {
            var convite = _context.Convites.FirstOrDefault(c => c.Token == token && !c.Usado && c.DataExpiracao > DateTime.Now);
            if (convite == null)
            {
                return View("ConviteInvalido");
            }
            ViewBag.Token = token;
            ViewBag.TurmaId = turmaId;
            ViewBag.Email = convite.Email;
            return View();
        }

        // POST: /Convite/Aceitar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceitar(string token, Guid turmaId, string senha, string confirmaSenha)
        {
            var convite = _context.Convites.FirstOrDefault(c => c.Token == token && !c.Usado && c.DataExpiracao > DateTime.Now);
            if (convite == null)
            {
                return View("ConviteInvalido");
            }
            if (string.IsNullOrEmpty(senha) || senha.Length < 6 || senha != confirmaSenha)
            {
                ModelState.AddModelError("", "Senha inválida ou não confere.");
                ViewBag.Token = token;
                ViewBag.TurmaId = turmaId;
                ViewBag.Email = convite.Email;
                return View();
            }
            // Cria usuário Identity
            var user = new IdentityUser { UserName = convite.Email, Email = convite.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, senha);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", string.Join(", ", result.Errors.Select(e => e.Description)));
                ViewBag.Token = token;
                ViewBag.TurmaId = turmaId;
                ViewBag.Email = convite.Email;
                return View();
            }
            // Adiciona claim de tipo de usuário
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("TipoUsuario", "Aluno"));
            // Salva usuário na tabela Usuarios (domínio)
            var usuario = new Usuario
            {
                Id = Guid.Parse(user.Id),
                Nome = convite.Email,
                Email = convite.Email,
                SenhaHash = user.PasswordHash,
                DataCriacao = DateTime.Now,
                Ativo = true,
                TipoUsuario = TipoUsuario.Aluno,
                TurmaId = turmaId
            };
            _context.Usuarios.Add(usuario);
            // Marca convite como usado
            convite.Usado = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }
    }
} 