using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.DTO.DB
{
    [Table("Questoes")]
    public class Questoes : TableBase
    {
        public Guid ProvaId { get; set; }
        public string Enunciado { get; set; }
        public int Ordem { get; set; }
        public int Pontos { get; set; } = 1;

        [ForeignKey("ProvaId")]
        public Provas Prova { get; set; }

        // Propriedade de navegação para alternativas
        public virtual ICollection<Alternativas> Alternativas { get; set; } = new List<Alternativas>();
    }
} 