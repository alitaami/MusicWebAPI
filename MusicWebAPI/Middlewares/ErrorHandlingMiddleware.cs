using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicWebAPI.Core.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;
using System.ComponentModel.DataAnnotations;
using ValidationException = MusicWebAPI.Domain.Base.Exceptions.CustomExceptions.ValidationException;
using MusicWebAPI.Domain.Interfaces.Services.MusicWebAPI.Domain.Interfaces;
using MusicWebAPI.Core.Resources;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerManager _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILoggerManager logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            // Read and log request body without disposing it
            var requestBody = await GetRequestBodyAsync(httpContext);
            _logger.LogInfo($"Request Body: {requestBody}");

            await _next(httpContext);
        }
        catch (Exception ex)
        {
            // Log the detailed exception
            await LogErrorAsync(httpContext, ex);

            // Handle the exception and return a proper response
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task<string> GetRequestBodyAsync(HttpContext httpContext)
    {
        if (httpContext.Request.ContentLength > 0)
        {
            httpContext.Request.EnableBuffering(); // Allow the body to be read multiple times
            httpContext.Request.Body.Position = 0; // Reset position to start reading

            using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                string body = await reader.ReadToEndAsync();
                httpContext.Request.Body.Position = 0; // Reset again for the next middleware to read
                return body;
            }
        }
        return string.Empty;
    }

    private async Task LogErrorAsync(HttpContext httpContext, Exception exception)
    {
        var logDetails = new
        {
            Timestamp = DateTime.UtcNow,
            HttpMethod = httpContext.Request.Method,
            RequestPath = httpContext.Request.Path,
            QueryString = httpContext.Request.QueryString.ToString(),
            Headers = httpContext.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString()),
            Body = await GetRequestBodyAsync(httpContext),
            ExceptionType = exception.GetType().ToString(),
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.Message,
        };

        _logger.LogError($"An unexpected error occurred: {logDetails}");
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            LogicException => StatusCodes.Status422UnprocessableEntity,
            InternalServerErrorException => StatusCodes.Status500InternalServerError,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ValidationException validationEx => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        #region ValidationException Process

        // If it's a ValidationException, extract the errors
        var errors = exception switch
        {
            ValidationException validationException => validationException.Errors,
            LogicException _ => new List<string>(), // Empty errors list for LogicException
            _ => new List<string>()

        };
        #endregion

        // Create the default error response for unhandled 500 errors
        string errorMessage = statusCode == StatusCodes.Status500InternalServerError
            ? Resource.GeneralUnhandledErrorText
            : exception.Message;

        var result = new ApiResult<object>(errorMessage, statusCode)
        {
            Errors = errors
        };

        return context.Response.WriteAsJsonAsync(result);
    }
}
