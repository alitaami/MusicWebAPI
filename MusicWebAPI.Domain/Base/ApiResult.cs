using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Core.Base
{
    public class ApiResult<T>
    {
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }

        // Constructor for success responses
        public ApiResult(T data, int statusCode = 200)
        {
            Data = data;
            IsSuccess = true;
            StatusCode = statusCode;
        }

        // Constructor for error responses
        public ApiResult(string errorMessage, int statusCode)
        {
            ErrorMessage = errorMessage;
            IsSuccess = false;
            StatusCode = statusCode;
        }

        // Implicit operator for success
        public static implicit operator ApiResult<T>(T data) => new ApiResult<T>(data);

        // Implicit operator for error (using exceptions)
        public static implicit operator ApiResult<T>(Exception ex) =>
            ex switch
            {
                NotFoundException => new ApiResult<T>(ex.Message, 404),
                BadRequestException => new ApiResult<T>(ex.Message, 400),
                LogicException => new ApiResult<T>(ex.Message, 422),
                InternalServerErrorException => new ApiResult<T>(ex.Message, 500),
                _ => new ApiResult<T>("An unexpected error occurred", 500)
            };
    }
}
