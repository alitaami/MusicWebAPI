using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicWebAPI.Core.Base;

namespace MusicWebAPI.API.Base
{
    /// <summary>
    /// Base class for handling standardized API responses.
    /// Provides helpers to return standardized API results with status codes and data.
    /// </summary>
    public abstract class ApiResponseBase
    {
        /// <summary>
        /// Standard success response with wrapped data.
        /// </summary>
        public static IResult Ok<T>(T data)
        {
            return Results.Ok(new ApiResult<T>(data, StatusCodes.Status200OK));
        }

        /// <summary>
        /// Standard error response for bad requests.
        /// </summary>
        public static IResult BadRequest(string errorMessage)
        {
            return Results.BadRequest(new ApiResult<string>(errorMessage, StatusCodes.Status400BadRequest));
        }

        /// <summary>
        /// Standard error response for not found errors.
        /// </summary>
        protected IResult NotFound(string errorMessage)
        {
            return Results.NotFound(new ApiResult<string>(errorMessage, StatusCodes.Status404NotFound));
        }

        /// <summary>
        /// Standard internal server error response.
        /// </summary>
        protected IResult InternalServerError(string errorMessage)
        {
            return Results.Problem(detail: errorMessage, statusCode: StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Standard unauthorized error response.
        /// </summary>
        protected IResult Unauthorized(string errorMessage)
        {
            return Results.Unauthorized();
        }
    }
}
