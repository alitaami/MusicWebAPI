using Configuration.Swagger;
using MusicWebAPI.API.Middlewares;
using Serilog;

public static class WebApplicationExtensions
{
    public static WebApplication Configure(this WebApplication app)
    {
        // Initialize Serilog with basic configuration
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/musicwebapi_log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseHsts();
            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerAndUI();
            }
            return app;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.Message);
            throw;
        }
    }
}
