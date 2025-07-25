using Swashbuckle.AspNetCore.Annotations;

namespace Project.DTO.Common
{
    public class FilterBase<T> where T : new()
    {
        [SwaggerSchema(ReadOnly = true)]
        public int Page { get; set; } = 1;

        [SwaggerSchema(ReadOnly = true)]
        public int Size { get; set; } = 0;

        [SwaggerSchema(ReadOnly = true)]
        public string? Search { get; set; }


        [SwaggerSchema(ReadOnly = true)]
        public DateTime? StartDate { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public DateTime? EndDate { get; set; }




        public T Filter { get; set; } = new T();

    }
}
