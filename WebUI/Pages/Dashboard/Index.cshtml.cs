using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebUI.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public string ApiMessage { get; set; }
        public string UserRole { get; set; }
        public string UserName { get; set; }
        public List<AlunoRankingDto> RankingAlunos { get; set; } = new();
        public List<TurmaComparacaoDto> ComparacaoTurmas { get; set; } = new();
        public List<AlunoRankingDto> RankingAlunosAtencao { get; set; } = new();
        public List<ConteudoRecomendadoDto> ConteudosRecomendados { get; set; } = new();

        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            // Buscar role da sessão
            var isProfessorStr = HttpContext.Session.GetString("IsProfessor");
            var userName = HttpContext.Session.GetString("UserName");
            UserName = string.IsNullOrEmpty(userName) ? "Usuário" : userName;
            bool isProfessor = false;
            if (!string.IsNullOrEmpty(isProfessorStr))
                bool.TryParse(isProfessorStr, out isProfessor);

            UserRole = isProfessor ? "Professor" : "Aluno";

            if (!isProfessor)
            {
                // Dashboard do Aluno
                ConteudosRecomendados = new List<ConteudoRecomendadoDto>
                {
                    new ConteudoRecomendadoDto {
                        Titulo = "Equações do 2º Grau",
                        Descricao = "Baseado na sua última prova, recomendamos revisar este conteúdo",
                        Tipo = "Videoaula",
                        Duracao = "15 min",
                        Prioridade = "Alta"
                    },
                    new ConteudoRecomendadoDto {
                        Titulo = "Geometria Básica",
                        Descricao = "Conceitos fundamentais que precisam de reforço",
                        Tipo = "Exercícios",
                        Duracao = "30 min",
                        Prioridade = "Média"
                    },
                    new ConteudoRecomendadoDto {
                        Titulo = "Funções do 1º Grau",
                        Descricao = "Praticar construção de gráficos",
                        Tipo = "Simulado",
                        Duracao = "45 min",
                        Prioridade = "Baixa"
                    }
                };
            }
            else
            {
                // Dashboard do Professor
                RankingAlunos = new List<AlunoRankingDto>
                {
                    new AlunoRankingDto { Nome = "Ana Souza", Nota = 9.8, Turma = "Turma A" },
                    new AlunoRankingDto { Nome = "Carlos Lima", Nota = 9.5, Turma = "Turma B" },
                    new AlunoRankingDto { Nome = "Beatriz Silva", Nota = 9.2, Turma = "Turma C" },
                    new AlunoRankingDto { Nome = "João Pedro", Nota = 8.9, Turma = "Turma A" },
                    new AlunoRankingDto { Nome = "Lucas Alves", Nota = 8.7, Turma = "Turma B" },
                    new AlunoRankingDto { Nome = "Mariana Costa", Nota = 8.5, Turma = "Turma C" },
                    new AlunoRankingDto { Nome = "Fernanda Dias", Nota = 8.3, Turma = "Turma A" },
                    new AlunoRankingDto { Nome = "Rafael Torres", Nota = 8.1, Turma = "Turma B" },
                    new AlunoRankingDto { Nome = "Juliana Rocha", Nota = 7.9, Turma = "Turma C" },
                    new AlunoRankingDto { Nome = "Gabriel Martins", Nota = 7.8, Turma = "Turma A" }
                };
                ComparacaoTurmas = new List<TurmaComparacaoDto>
                {
                    new TurmaComparacaoDto { Nome = "Turma A", Media = 8.7 },
                    new TurmaComparacaoDto { Nome = "Turma B", Media = 7.9 },
                    new TurmaComparacaoDto { Nome = "Turma C", Media = 8.2 }
                };
                RankingAlunosAtencao = new List<AlunoRankingDto>
                {
                    new AlunoRankingDto { Nome = "Pedro Henrique", Nota = 5.2, Turma = "Turma B", Observacao = "Reforçar: Equações do 2º grau" },
                    new AlunoRankingDto { Nome = "Larissa Gomes", Nota = 5.5, Turma = "Turma C", Observacao = "Praticar: Geometria básica" },
                    new AlunoRankingDto { Nome = "Vinícius Souza", Nota = 5.7, Turma = "Turma A", Observacao = "Revisar: Funções do 1º grau" },
                    new AlunoRankingDto { Nome = "Amanda Ribeiro", Nota = 5.9, Turma = "Turma B", Observacao = "Assistir: Videoaula sobre gráficos" },
                    new AlunoRankingDto { Nome = "Thiago Lopes", Nota = 6.0, Turma = "Turma C", Observacao = "Ler: Resumo de fórmulas" }
                };
            }

            return Page();
        }

        public class AlunoRankingDto
        {
            public string Nome { get; set; }
            public double Nota { get; set; }
            public string Turma { get; set; }
            public string Observacao { get; set; }
        }
        public class TurmaComparacaoDto
        {
            public string Nome { get; set; }
            public double Media { get; set; }
        }
        public class ConteudoRecomendadoDto
        {
            public string Titulo { get; set; }
            public string Descricao { get; set; }
            public string Tipo { get; set; }
            public string Duracao { get; set; }
            public string Prioridade { get; set; }
        }
    }
} 