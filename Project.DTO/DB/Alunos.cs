using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.DTO.DB
{
    [Table("Alunos")]
    public class Alunos : TableBase
    {
        public Guid ProfessorId { get; set; }
        public Guid AlunoId { get; set; }
        public Guid TurmaId { get; set; }

        [ForeignKey("ProfessorId")]
        public TB_ADM_USUARIO ProfessoresFK { get; set; }

        [ForeignKey("AlunoId")]
        public TB_ADM_USUARIO AlunosFK { get; set; }

        [ForeignKey("TurmaId")]
        public Turmas TurmasFK { get; set; }

    }
}
