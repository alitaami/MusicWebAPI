namespace MusicWebAPI.Core.Base
{
    public class ApiResult<T>
    {
        public T? Data { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; } 

        public ApiResult(T? data, int statusCode = 200)
        {
            Data = data;
            IsSuccess = true;
            StatusCode = statusCode;
            Errors = new List<string>(); 
        }

        public ApiResult(string errorMessage, int statusCode, bool isSuccess = false)
        { 
            ErrorMessage = errorMessage;
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Errors = new List<string>(); 
        }
    }
}
