using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.DTO.DB
{
    [Table("Turmas")]
    public class Turmas : TableBase
    {
        public Guid UsuarioId { get; set; }

        [Required]
        public string Disciplina { get; set; }
        [Required]
        public string Serie { get; set; }
        [Required]
        public string Turno { get; set; }
        [Required]
        public int Ano { get; set; }

        [ForeignKey("UsuarioId")]
        public TB_ADM_USUARIO Usuarios { get; set; }

    }
}
