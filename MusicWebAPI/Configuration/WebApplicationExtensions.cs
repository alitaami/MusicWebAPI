using Configuration.Swagger;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.API.Endpoints;
using MusicWebAPI.API.Middlewares;
using MusicWebAPI.Infrastructure.Data.Context;
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
            // Apply database migrations automatically (synchronously)
            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    var context = services.GetRequiredService<MusicDbContext>();
            //    context.Database.MigrateAsync().GetAwaiter().GetResult(); // Apply migrations synchronously
            //}

            //// Seed data (optional) (synchronously)
            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    var context = services.GetRequiredService<MusicDbContext>();
            //    SeedData.Initialize(services, context).GetAwaiter().GetResult(); // Seed data synchronously
            //}

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

            // Register the Minimal API routes
            UserEndpoints.RegisterUserEndpoints(app); // Call the method that registers endpoints

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
