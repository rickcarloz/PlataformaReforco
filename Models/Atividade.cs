using System;
using System.Collections.Generic;

namespace PlataformaReforco.Models
{
    public class Atividade
    {
        public Guid Id { get; set; }
        public Guid TurmaId { get; set; }
        public Turma Turma { get; set; }
        public Bimestre Bimestre { get; set; }
        public string Titulo { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool NotasLiberadas { get; set; } = false;
        public ICollection<Questao> Questoes { get; set; }
    }

    public enum Bimestre
    {
        Primeiro = 1,
        Segundo = 2,
        Terceiro = 3,
        Quarto = 4
    }
} 