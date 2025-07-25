using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Project.DTO.DB;

namespace Project.DAL
{
    public class DataBaseContext : DbContext
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _context;
        private readonly ILoggerFactory _loggerFactory;


        public DataBaseContext(IConfiguration config, IHttpContextAccessor context) : base()
        {
            _config = config;
            _context = context;

            bool ShowLogginDataBase = bool.TryParse(_config["ShowLogginDataBase"], out bool LogginDataBase) ? LogginDataBase : false;
            if (ShowLogginDataBase)
            {
                _loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddSimpleConsole(o =>
                    {
                        o.IncludeScopes = true;
                        o.TimestampFormat = "HH:mm:ss ";
                        o.SingleLine = false;


                    });
                });
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_config.GetConnectionString("DataBaseConnection"));
            optionsBuilder.UseLoggerFactory(_loggerFactory);

        }

        public override int SaveChanges()
        {
            // SaveAuditoria();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //  SaveAuditoria();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            modelBuilder.Entity<TB_ADM_USUARIO>().HasIndex(x => x.USUARIO).IsUnique();

            // Configurações para o sistema de provas
            modelBuilder.Entity<Provas>()
                .HasOne(p => p.Turma)
                .WithMany()
                .HasForeignKey(p => p.TurmaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provas>()
                .HasOne(p => p.Professor)
                .WithMany()
                .HasForeignKey(p => p.ProfessorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Questoes>()
                .HasOne(q => q.Prova)
                .WithMany()
                .HasForeignKey(q => q.ProvaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alternativas>()
                .HasOne(a => a.Questao)
                .WithMany()
                .HasForeignKey(a => a.QuestaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProvasAlunos>()
                .HasOne(pa => pa.Prova)
                .WithMany()
                .HasForeignKey(pa => pa.ProvaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProvasAlunos>()
                .HasOne(pa => pa.Aluno)
                .WithMany()
                .HasForeignKey(pa => pa.AlunoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RespostasAlunos>()
                .HasOne(ra => ra.ProvaAluno)
                .WithMany()
                .HasForeignKey(ra => ra.ProvaAlunoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RespostasAlunos>()
                .HasOne(ra => ra.Questao)
                .WithMany()
                .HasForeignKey(ra => ra.QuestaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RespostasAlunos>()
                .HasOne(ra => ra.Alternativa)
                .WithMany()
                .HasForeignKey(ra => ra.AlternativaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações para propriedades de navegação
            modelBuilder.Entity<Provas>()
                .HasMany(p => p.Questoes)
                .WithOne(q => q.Prova)
                .HasForeignKey(q => q.ProvaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Provas>()
                .HasMany(p => p.ProvasAlunos)
                .WithOne(pa => pa.Prova)
                .HasForeignKey(pa => pa.ProvaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Questoes>()
                .HasMany(q => q.Alternativas)
                .WithOne(a => a.Questao)
                .HasForeignKey(a => a.QuestaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProvasAlunos>()
                .HasMany(pa => pa.RespostasAlunos)
                .WithOne(ra => ra.ProvaAluno)
                .HasForeignKey(ra => ra.ProvaAlunoId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);

        }
        public DbSet<TB_ADM_USUARIO> TB_ADM_USUARIO { get; set; }
        public DbSet<Turmas> Turmas { get; set; }
        public DbSet<Alunos> Alunos { get; set; }
        public DbSet<Provas> Provas { get; set; }
        public DbSet<Questoes> Questoes { get; set; }
        public DbSet<Alternativas> Alternativas { get; set; }
        public DbSet<ProvasAlunos> ProvasAlunos { get; set; }
        public DbSet<RespostasAlunos> RespostasAlunos { get; set; }

    }
}
