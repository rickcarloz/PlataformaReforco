using System;
using System.Collections.Generic;

namespace PlataformaReforco.Models
{
    public class Turma
    {
        public Guid Id { get; set; }
        public string Disciplina { get; set; }
        public string Serie { get; set; }
        public string Turno { get; set; }
        public int Ano { get; set; }
        public string ProfessorId { get; set; }
        public Usuario Professor { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool Ativa { get; set; } = true;
        public ICollection<Usuario> Alunos { get; set; }
        public ICollection<Atividade> Atividades { get; set; }
    }
} 