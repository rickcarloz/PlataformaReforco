using System;
using System.ComponentModel.DataAnnotations;

namespace PlataformaReforco.Models
{
    public class Convite
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime DataExpiracao { get; set; }
        public bool Usado { get; set; } = false;
    }
} 