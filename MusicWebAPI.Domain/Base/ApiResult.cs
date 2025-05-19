namespace MusicWebAPI.Core.Base
{
    public class ApiResult<T>
    {
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; } // Add this field for validation errors

        // Constructor for success responses
        public ApiResult(T data, int statusCode = 200)
        {
            Data = data;
            IsSuccess = true;
            StatusCode = statusCode;
            Errors = new List<string>(); // Initialize as empty list
        }

        // Constructor for error responses
        public ApiResult(string errorMessage, int statusCode)
        { 
            ErrorMessage = errorMessage;
            IsSuccess = false;
            StatusCode = statusCode;
            Errors = new List<string>(); // Initialize as empty list
        }
    }
}
