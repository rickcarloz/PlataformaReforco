using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public enum Bimestre
    {
        Primeiro = 1,
        Segundo = 2,
        Terceiro = 3,
        Quarto = 4
    }

    public class Atividade
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TurmaId { get; set; }
        [ForeignKey("TurmaId")]
        public Turma Turma { get; set; }

        [Required]
        public Bimestre Bimestre { get; set; }

        [Required]
        public string Titulo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool NotasLiberadas { get; set; } = false;

        public ICollection<Questao> Questoes { get; set; }
    }
} 