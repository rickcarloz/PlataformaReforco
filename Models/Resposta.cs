using System;

namespace PlataformaReforco.Models
{
    public class Resposta
    {
        public Guid Id { get; set; }
        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public Guid QuestaoId { get; set; }
        public Questao Questao { get; set; }
        public string RespostaAluno { get; set; }
        public bool? Correta { get; set; } // null = não corrigida ainda
        public DateTime DataEnvio { get; set; } = DateTime.Now;
    }
} 