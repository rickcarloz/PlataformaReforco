using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public enum TipoUsuario
    {
        Professor = 1,
        Aluno = 2
    }

    public class Usuario
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string SenhaHash { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;

        [Required]
        public TipoUsuario TipoUsuario { get; set; }

        // Se for aluno
        public Guid? TurmaId { get; set; }
        [ForeignKey("TurmaId")]
        public Turma? Turma { get; set; }
    }
} 