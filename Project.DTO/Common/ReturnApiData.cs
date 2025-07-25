namespace Project.DTO.Common
{
    public class ReturnApiData<T>
    {
        public int StatusCod { get; set; }
        public bool Success { get; set; }
        public object Message { get; set; }
        public T Data { get; set; }
    }
}
