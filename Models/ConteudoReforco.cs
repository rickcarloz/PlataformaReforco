using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaReforco.Models
{
    public class ConteudoReforco
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Required]
        public Guid AtividadeId { get; set; }
        [ForeignKey("AtividadeId")]
        public Atividade Atividade { get; set; }

        [Required]
        public string TextoGerado { get; set; }

        public string? LinkExtra { get; set; }

        public string? Tipo { get; set; } // texto, video, link, etc

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
} 