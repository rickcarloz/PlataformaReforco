using System;
using System.Collections.Generic;

namespace PlataformaReforco.Models
{
    public class Questao
    {
        public Guid Id { get; set; }
        public Guid AtividadeId { get; set; }
        public Atividade Atividade { get; set; }
        public TipoQuestao Tipo { get; set; }
        public string Enunciado { get; set; }
        // Para questões de escolha, armazene as alternativas em JSON
        public string AlternativasJson { get; set; }
        public string RespostaCorreta { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }

    public enum TipoQuestao
    {
        Dissertativa = 1,
        Escolha = 2
    }
} 