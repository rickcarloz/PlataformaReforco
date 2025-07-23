using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public class Usuario : IdentityUser
    {
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
        public TipoUsuario TipoUsuario { get; set; }
        // Se for aluno, relaciona com Turma
        public Guid? TurmaId { get; set; }
        public Turma Turma { get; set; }
    }

    public enum TipoUsuario
    {
        Professor = 1,
        Aluno = 2
    }
} 