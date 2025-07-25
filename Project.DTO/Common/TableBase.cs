using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Project.DTO.Common
{
    public class TableBase
    {
        [Key]
        [SwaggerSchema(ReadOnly = true), JsonProperty(Order = int.MinValue)]
        public Guid ID { get; set; }

        [SwaggerSchema(ReadOnly = true), JsonProperty(Order = int.MinValue)]
        public DateTimeOffset DATA_CRIACAO { get; set; }

        [SwaggerSchema(ReadOnly = true), JsonProperty(Order = int.MinValue)]
        public string? USUARIO_CRIACAO { get; set; }

        [SwaggerSchema(ReadOnly = true), JsonProperty(Order = int.MinValue)]
        public DateTimeOffset? DATA_MODIFICACAO { get; set; }

        [SwaggerSchema(ReadOnly = true), JsonProperty(Order = int.MinValue)]
        public string? USUARIO_MODIFICACAO { get; set; }

        [SwaggerSchema(ReadOnly = true), JsonProperty(Order = int.MinValue)]
        public bool ATIVO { get; set; }
    }


    public class BaseNoEntity
    {

    }
}
