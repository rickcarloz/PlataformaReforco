using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public enum TipoQuestao
    {
        Dissertativa = 1,
        MultiplaEscolha = 2
    }

    public class Questao
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AtividadeId { get; set; }
        [ForeignKey("AtividadeId")]
        public Atividade Atividade { get; set; }

        [Required]
        public string Enunciado { get; set; }

        [Required]
        public TipoQuestao Tipo { get; set; }

        // Para múltipla escolha
        public string? Alternativas { get; set; } // Pode ser JSON ou separado por ;

        public string? RespostaCorreta { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
} 