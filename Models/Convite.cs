using System;
using System.ComponentModel.DataAnnotations;

namespace PlataformaReforco.Models
{
    public class Convite
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public Guid TurmaId { get; set; }
        public Turma Turma { get; set; }
        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime DataExpiracao { get; set; }
        public bool Aceito { get; set; } = false;
    }
} 