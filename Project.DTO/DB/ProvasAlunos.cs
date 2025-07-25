using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.DTO.DB
{
    [Table("ProvasAlunos")]
    public class ProvasAlunos : TableBase
    {
        public Guid ProvaId { get; set; }
        public Guid AlunoId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Pontuacao { get; set; }
        public bool Concluida { get; set; } = false;
        public bool Aprovada { get; set; } = false;
        public string? RecomendacoesChatGPT { get; set; }
        public bool RecomendacoesLiberadas { get; set; } = false;

        [ForeignKey("ProvaId")]
        public Provas Prova { get; set; }

        [ForeignKey("AlunoId")]
        public TB_ADM_USUARIO Aluno { get; set; }

        // Propriedade de navegação para respostas
        public virtual ICollection<RespostasAlunos> RespostasAlunos { get; set; } = new List<RespostasAlunos>();
    }
} 