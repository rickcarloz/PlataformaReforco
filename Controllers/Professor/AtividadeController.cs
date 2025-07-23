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
    public class AtividadeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtividadeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Professor/Atividade
        public IActionResult Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var atividades = _context.Atividades
                .Where(a => a.Turma.ProfessorId.ToString() == userId)
                .OrderByDescending(a => a.DataCriacao)
                .ToList();
            return View(atividades);
        }

        // GET: Professor/Atividade/Create
        public IActionResult Create()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            ViewBag.Turmas = _context.Turmas.Where(t => t.ProfessorId.ToString() == userId).ToList();
            ViewBag.Bimestres = Enum.GetValues(typeof(Bimestre));
            return View();
        }

        // POST: Professor/Atividade/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid turmaId, Bimestre bimestre, string titulo)
        {
            if (turmaId == Guid.Empty || string.IsNullOrEmpty(titulo))
            {
                ModelState.AddModelError("", "Preencha todos os campos.");
                var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                ViewBag.Turmas = _context.Turmas.Where(t => t.ProfessorId.ToString() == userId).ToList();
                ViewBag.Bimestres = Enum.GetValues(typeof(Bimestre));
                return View();
            }
            var atividade = new Atividade
            {
                TurmaId = turmaId,
                Bimestre = bimestre,
                Titulo = titulo,
                DataCriacao = DateTime.Now,
                NotasLiberadas = false
            };
            _context.Atividades.Add(atividade);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
} 