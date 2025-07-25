using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.DTO.DB
{
    [Table("Alternativas")]
    public class Alternativas : TableBase
    {
        public Guid QuestaoId { get; set; }
        public string Texto { get; set; }
        public string Letra { get; set; } // A, B, C, D, E
        public bool Correta { get; set; }

        [ForeignKey("QuestaoId")]
        public Questoes Questao { get; set; }
    }
} 