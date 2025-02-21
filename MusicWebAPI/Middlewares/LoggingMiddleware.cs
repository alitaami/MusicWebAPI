using MusicWebAPI.Domain.Interfaces.Services.MusicWebAPI.Domain.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace MusicWebAPI.API.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerManager _logger;

        public LoggingMiddleware(RequestDelegate next, ILoggerManager logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // Log request details (method, url, headers)
            _logger.LogInfo($"Request: {request.Method} {request.Path} - Headers: {JsonSerializer.Serialize(request.Headers)}");

            // Create a stopwatch to measure the request duration
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Capture the response body by using a MemoryStream
                var originalResponseBody = response.Body;
                using (var responseBodyStream = new MemoryStream())
                {
                    response.Body = responseBodyStream;

                    await _next(context);  // Call the next middleware

                    // Log response status code after the response is finished
                    _logger.LogInfo($"Response: {response.StatusCode}");

                    // Copy the response content back to the original response stream
                    await responseBodyStream.CopyToAsync(originalResponseBody);
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the request
                _logger.LogError($"Exception: {ex.Message}");
                throw;  // Re-throw the exception to maintain the stack trace
            }
            finally
            {
                stopwatch.Stop();
                // Log the time it took to process the request
                _logger.LogInfo($"Request processed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
