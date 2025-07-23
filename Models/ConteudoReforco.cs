using System;

namespace PlataformaReforco.Models
{
    public class ConteudoReforco
    {
        public Guid Id { get; set; }
        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public Guid AtividadeId { get; set; }
        public Atividade Atividade { get; set; }
        public string TextoGerado { get; set; }
        public string LinkExtra { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
} 