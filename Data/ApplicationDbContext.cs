using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PlataformaReforco.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlataformaReforco.Models.Usuario> Usuarios { get; set; }
    public DbSet<PlataformaReforco.Models.Turma> Turmas { get; set; }
    public DbSet<PlataformaReforco.Models.Atividade> Atividades { get; set; }
    public DbSet<PlataformaReforco.Models.Questao> Questoes { get; set; }
    public DbSet<PlataformaReforco.Models.Resposta> Respostas { get; set; }
    public DbSet<PlataformaReforco.Models.ConteudoReforco> ConteudosReforco { get; set; }
    public DbSet<PlataformaReforco.Models.Convite> Convites { get; set; }
}
