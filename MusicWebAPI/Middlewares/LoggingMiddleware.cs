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

            _logger.LogInfo($"Request: {request.Method} {request.Path} - Headers: {JsonSerializer.Serialize(request.Headers)}");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var originalResponseBody = response.Body; // Store original response body
            using var responseBodyStream = new MemoryStream();

            try
            {
                response.Body = responseBodyStream;  // Swap response body with memory stream

                await _next(context);  // Call the next middleware

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                string responseBodyText = await new StreamReader(responseBodyStream).ReadToEndAsync();

                _logger.LogInfo($"Response: {response.StatusCode} - Body: {responseBodyText}");

                // Reset position to write back to original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                response.Body = originalResponseBody; // Restore the original response body
                stopwatch.Stop();
                _logger.LogInfo($"Request processed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
