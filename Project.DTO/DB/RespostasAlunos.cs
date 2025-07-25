using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.DTO.DB
{
    [Table("RespostasAlunos")]
    public class RespostasAlunos : TableBase
    {
        public Guid ProvaAlunoId { get; set; }
        public Guid QuestaoId { get; set; }
        public Guid AlternativaId { get; set; }
        public bool Correta { get; set; }
        public int PontosObtidos { get; set; }

        [ForeignKey("ProvaAlunoId")]
        public ProvasAlunos ProvaAluno { get; set; }

        [ForeignKey("QuestaoId")]
        public Questoes Questao { get; set; }

        [ForeignKey("AlternativaId")]
        public Alternativas Alternativa { get; set; }
    }
} 