using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.DTO.DB
{
    [Table("Provas")]
    public class Provas : TableBase
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public Guid TurmaId { get; set; }
        public Guid ProfessorId { get; set; }
        public int TempoLimite { get; set; } // em minutos
        public bool Ativa { get; set; } = true;

        [ForeignKey("TurmaId")]
        public Turmas Turma { get; set; }

        [ForeignKey("ProfessorId")]
        public TB_ADM_USUARIO Professor { get; set; }

        // Propriedades de navegação
        public virtual ICollection<Questoes> Questoes { get; set; } = new List<Questoes>();
        public virtual ICollection<ProvasAlunos> ProvasAlunos { get; set; } = new List<ProvasAlunos>();
    }
} 