using MusicWebAPI.Core.Base;
using Microsoft.AspNetCore.Http;

namespace MusicWebAPI.API.Base
{
    /// <summary>
    /// Base class for handling standardized API responses.
    /// Provides helpers to return standardized API result with status codes and data.
    /// </summary>
    public abstract class ApiResponseBase
    {
        /// <summary>
        /// Standard success response with wrapped data.
        /// </summary>
        /// <param name="data">The data to be returned in the response.</param>
        /// <param name="statusCode">The status code to be returned (default is 200 OK).</param>
        /// <typeparam name="T">The type of data to be returned.</typeparam>
        /// <returns>A standardized response wrapped in ApiResult.</returns>
        protected IResult Ok<T>(T data, int statusCode = StatusCodes.Status200OK)
        {
            var apiResult = new ApiResult<T>(data, statusCode);
            return Results.Json(apiResult, statusCode: statusCode);  // Use Results.Json and pass the status code
        }

        /// <summary>
        /// Standard error response for bad request with custom message.
        /// </summary>
        /// <param name="errorMessage">The error message to be returned.</param>
        /// <param name="statusCode">The status code (default is 400 Bad Request).</param>
        /// <typeparam name="T">The type of data to be returned (in this case, null or empty).</typeparam>
        /// <returns>A standardized error response wrapped in ApiResult.</returns>
        protected IResult BadRequest<T>(string errorMessage, int statusCode = StatusCodes.Status400BadRequest)
        {
            var apiResult = new ApiResult<T>(errorMessage, statusCode);
            return Results.Json(apiResult, statusCode: statusCode);  // Use Results.Json and pass the status code
        }

        /// <summary>
        /// Standard error response for not found error.
        /// </summary>
        /// <param name="errorMessage">The error message to be returned.</param>
        /// <typeparam name="T">The type of data to be returned (in this case, null or empty).</typeparam>
        /// <returns>A standardized not found response wrapped in ApiResult.</returns>
        protected IResult NotFound<T>(string errorMessage)
        {
            var apiResult = new ApiResult<T>(errorMessage, StatusCodes.Status404NotFound);
            return Results.Json(apiResult, statusCode: StatusCodes.Status404NotFound);  // Use Results.Json and pass the status code
        }

        /// <summary>
        /// Standard internal server error response.
        /// </summary>
        /// <param name="errorMessage">The error message to be returned.</param>
        /// <typeparam name="T">The type of data to be returned (in this case, null or empty).</typeparam>
        /// <returns>A standardized internal server error response wrapped in ApiResult.</returns>
        protected IResult InternalServerError<T>(string errorMessage)
        {
            var apiResult = new ApiResult<T>(errorMessage, StatusCodes.Status500InternalServerError);
            return Results.Json(apiResult, statusCode: StatusCodes.Status500InternalServerError);  // Use Results.Json and pass the status code
        }

        /// <summary>
        /// Helper method to return an unauthorized response with a custom message.
        /// </summary>
        /// <param name="errorMessage">The error message to be returned.</param>
        /// <typeparam name="T">The type of data to be returned (in this case, null or empty).</typeparam>
        /// <returns>A standardized unauthorized response wrapped in ApiResult.</returns>
        protected IResult Unauthorized<T>(string errorMessage)
        {
            var apiResult = new ApiResult<T>(errorMessage, StatusCodes.Status401Unauthorized);
            return Results.Json(apiResult, statusCode: StatusCodes.Status401Unauthorized);  // Use Results.Json and pass the status code
        }
    }
}
