using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public class Resposta
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        public Guid QuestaoId { get; set; }
        [ForeignKey("QuestaoId")]
        public Questao Questao { get; set; }

        [Required]
        public Guid AtividadeId { get; set; }
        [ForeignKey("AtividadeId")]
        public Atividade Atividade { get; set; }

        [Required]
        public string RespostaAluno { get; set; }

        public bool? Correta { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;
    }
} 