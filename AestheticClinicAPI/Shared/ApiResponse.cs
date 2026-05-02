namespace AestheticClinicAPI.Shared
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail(string error, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error,
                Message = message
            };
        }
    }
}