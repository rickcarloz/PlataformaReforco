using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public class Turma
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Disciplina { get; set; }

        [Required]
        public string Serie { get; set; }

        [Required]
        public string Turno { get; set; }

        [Required]
        public int Ano { get; set; }

        [Required]
        public Guid ProfessorId { get; set; }
        [ForeignKey("ProfessorId")]
        public Usuario Professor { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool Ativa { get; set; } = true;

        public ICollection<Atividade> Atividades { get; set; }
    }
} 