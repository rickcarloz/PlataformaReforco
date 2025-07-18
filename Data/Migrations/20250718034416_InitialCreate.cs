using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaReforco.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Convites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Usado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Atividades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TurmaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Bimestre = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NotasLiberadas = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AtividadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Enunciado = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Alternativas = table.Column<string>(type: "TEXT", nullable: true),
                    RespostaCorreta = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questoes_Atividades_AtividadeId",
                        column: x => x.AtividadeId,
                        principalTable: "Atividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConteudosReforco",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AtividadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TextoGerado = table.Column<string>(type: "TEXT", nullable: false),
                    LinkExtra = table.Column<string>(type: "TEXT", nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConteudosReforco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConteudosReforco_Atividades_AtividadeId",
                        column: x => x.AtividadeId,
                        principalTable: "Atividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Respostas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AtividadeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RespostaAluno = table.Column<string>(type: "TEXT", nullable: false),
                    Correta = table.Column<bool>(type: "INTEGER", nullable: true),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Respostas_Atividades_AtividadeId",
                        column: x => x.AtividadeId,
                        principalTable: "Atividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Respostas_Questoes_QuestaoId",
                        column: x => x.QuestaoId,
                        principalTable: "Questoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turmas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Disciplina = table.Column<string>(type: "TEXT", nullable: false),
                    Serie = table.Column<string>(type: "TEXT", nullable: false),
                    Turno = table.Column<string>(type: "TEXT", nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    ProfessorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turmas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaHash = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    TipoUsuario = table.Column<int>(type: "INTEGER", nullable: false),
                    TurmaId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_TurmaId",
                table: "Atividades",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosReforco_AtividadeId",
                table: "ConteudosReforco",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ConteudosReforco_UsuarioId",
                table: "ConteudosReforco",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Questoes_AtividadeId",
                table: "Questoes",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Respostas_AtividadeId",
                table: "Respostas",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Respostas_QuestaoId",
                table: "Respostas",
                column: "QuestaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Respostas_UsuarioId",
                table: "Respostas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_ProfessorId",
                table: "Turmas",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TurmaId",
                table: "Usuarios",
                column: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atividades_Turmas_TurmaId",
                table: "Atividades",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConteudosReforco_Usuarios_UsuarioId",
                table: "ConteudosReforco",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Respostas_Usuarios_UsuarioId",
                table: "Respostas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turmas_Usuarios_ProfessorId",
                table: "Turmas",
                column: "ProfessorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Turmas_TurmaId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "ConteudosReforco");

            migrationBuilder.DropTable(
                name: "Convites");

            migrationBuilder.DropTable(
                name: "Respostas");

            migrationBuilder.DropTable(
                name: "Questoes");

            migrationBuilder.DropTable(
                name: "Atividades");

            migrationBuilder.DropTable(
                name: "Turmas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
