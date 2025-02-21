using MusicWebAPI.Core.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Convert the exception into a consistent ApiResult response
        ApiResult<object> result = exception switch
        {
            NotFoundException notFoundEx => new ApiResult<object>(notFoundEx.Message, 404),
            BadRequestException badRequestEx => new ApiResult<object>(badRequestEx.Message, 400),
            LogicException logicEx => new ApiResult<object>(logicEx.Message, 422),
            InternalServerErrorException internalEx => new ApiResult<object>(internalEx.Message, 500),
            _ => new ApiResult<object>("An unexpected error occurred", 500),
        };

        context.Response.StatusCode = result.StatusCode;
        return context.Response.WriteAsJsonAsync(result);
    }
}
