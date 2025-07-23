using Microsoft.AspNetCore.Mvc;
using PlataformaReforco.Controllers.Filters;
using PlataformaReforco.Data;
using PlataformaReforco.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PlataformaReforco.Controllers.Professor
{
    [TipoUsuarioAuthorize("Professor")]
    public class ConviteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConviteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Professor/Convite
        public IActionResult Index()
        {
            var convites = _context.Convites.OrderByDescending(c => c.DataEnvio).ToList();
            return View(convites);
        }

        // GET: Professor/Convite/Create
        public IActionResult Create()
        {
            ViewBag.Turmas = _context.Turmas.ToList();
            return View();
        }

        // POST: Professor/Convite/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, Guid turmaId)
        {
            if (string.IsNullOrEmpty(email) || turmaId == Guid.Empty)
            {
                ModelState.AddModelError("", "Preencha todos os campos.");
                ViewBag.Turmas = _context.Turmas.ToList();
                return View();
            }

            var token = Guid.NewGuid().ToString();
            var convite = new Convite
            {
                Email = email,
                Token = token,
                DataEnvio = DateTime.Now,
                DataExpiracao = DateTime.Now.AddHours(48),
                Aceito = false
            };
            _context.Convites.Add(convite);
            await _context.SaveChangesAsync();

            // Simular envio de e-mail (exibir link na tela)
            ViewBag.LinkConvite = Url.Action("Aceitar", "Convite", new { area = "", token = token, turmaId = turmaId }, Request.Scheme);
            ViewBag.Turmas = _context.Turmas.ToList();
            ViewBag.Email = email;
            return View("ConviteEnviado");
        }
    }
} 