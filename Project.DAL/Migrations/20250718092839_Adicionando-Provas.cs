using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoProvas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Provas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TurmaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfessorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempoLimite = table.Column<int>(type: "int", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    DATA_CRIACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    USUARIO_CRIACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATA_MODIFICACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USUARIO_MODIFICACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Provas_TB_ADM_USUARIO_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "TB_ADM_USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Provas_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProvasAlunos",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlunoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Pontuacao = table.Column<int>(type: "int", nullable: false),
                    Concluida = table.Column<bool>(type: "bit", nullable: false),
                    Aprovada = table.Column<bool>(type: "bit", nullable: false),
                    RecomendacoesChatGPT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecomendacoesLiberadas = table.Column<bool>(type: "bit", nullable: false),
                    DATA_CRIACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    USUARIO_CRIACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATA_MODIFICACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USUARIO_MODIFICACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvasAlunos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProvasAlunos_Provas_ProvaId",
                        column: x => x.ProvaId,
                        principalTable: "Provas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProvasAlunos_TB_ADM_USUARIO_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "TB_ADM_USUARIO",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questoes",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Enunciado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Pontos = table.Column<int>(type: "int", nullable: false),
                    DATA_CRIACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    USUARIO_CRIACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATA_MODIFICACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USUARIO_MODIFICACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questoes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Questoes_Provas_ProvaId",
                        column: x => x.ProvaId,
                        principalTable: "Provas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alternativas",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Letra = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correta = table.Column<bool>(type: "bit", nullable: false),
                    DATA_CRIACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    USUARIO_CRIACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATA_MODIFICACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USUARIO_MODIFICACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alternativas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Alternativas_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespostasAlunos",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvaAlunoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlternativaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Correta = table.Column<bool>(type: "bit", nullable: false),
                    PontosObtidos = table.Column<int>(type: "int", nullable: false),
                    DATA_CRIACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    USUARIO_CRIACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATA_MODIFICACAO = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    USUARIO_MODIFICACAO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATIVO = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespostasAlunos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RespostasAlunos_Alternativas_AlternativaId",
                        column: x => x.AlternativaId,
                        principalTable: "Alternativas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespostasAlunos_ProvasAlunos_ProvaAlunoId",
                        column: x => x.ProvaAlunoId,
                        principalTable: "ProvasAlunos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespostasAlunos_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alternativas_QuestaoId",
                table: "Alternativas",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Provas_ProfessorId",
                table: "Provas",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Provas_TurmaId",
                table: "Provas",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvasAlunos_AlunoId",
                table: "ProvasAlunos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvasAlunos_ProvaId",
                table: "ProvasAlunos",
                column: "ProvaId");

            migrationBuilder.CreateIndex(
                name: "IX_Questoes_ProvaId",
                table: "Questoes",
                column: "ProvaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasAlunos_AlternativaId",
                table: "RespostasAlunos",
                column: "AlternativaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasAlunos_ProvaAlunoId",
                table: "RespostasAlunos",
                column: "ProvaAlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_RespostasAlunos_QuestaoId",
                table: "RespostasAlunos",
                column: "QuestaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RespostasAlunos");

            migrationBuilder.DropTable(
                name: "Alternativas");

            migrationBuilder.DropTable(
                name: "ProvasAlunos");

            migrationBuilder.DropTable(
                name: "Questoes");

            migrationBuilder.DropTable(
                name: "Provas");
        }
    }
}
