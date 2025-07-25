using Newtonsoft.Json;
using Project.DTO.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.DTO.DB
{
    [Table("TB_ADM_USUARIO")]
    public class TB_ADM_USUARIO : TableBase
    {

        [Required]
        public string USUARIO { get; set; }
        [Required]
        public string NOME { get; set; }
        [Required]
        public string EMAIL { get; set; }
        public bool Professor { get; set; }

        public bool FORCE_ALTERAR_SENHA { get; set; }

        [JsonIgnore]
        public string? PASSWORD_HASH { get; set; }

        [JsonIgnore]
        public string? PASSWORD_SALT { get; set; }

    }
}
