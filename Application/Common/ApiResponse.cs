namespace Application.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public List<string> Errors { get; init; } = [];

        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string error) =>
            new() { Success = false, Errors = [error] };

        public ApiResponse(T data)
        {
            Success = true;
            Data = data;
            Message = "İşlem başarıyla tamamlandı.";
        }

        public ApiResponse() { }

    }
}
