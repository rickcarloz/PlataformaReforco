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
    public class TurmaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TurmaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Professor/Turma
        public IActionResult Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var turmas = _context.Turmas.Where(t => t.ProfessorId.ToString() == userId).ToList();
            return View(turmas);
        }

        // GET: Professor/Turma/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Professor/Turma/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Disciplina,Serie,Turno,Ano")] Turma turma)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                turma.ProfessorId = Guid.Parse(userId);
                turma.DataCriacao = DateTime.Now;
                turma.Ativa = true;
                _context.Add(turma);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(turma);
        }
    }
} 