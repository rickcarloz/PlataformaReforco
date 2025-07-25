using System.Text.Json.Serialization;

namespace Project.DTO.Common
{
    public class PaginateModel<T>
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
