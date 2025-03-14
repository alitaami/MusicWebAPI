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
        try
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

            app.UseHttpsRedirection();
            app.UseHsts();

            app.UseCors("AllowAll");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Register the Minimal API routes
            UserEndpoints.RegisterUserEndpoints(app); // Call the method that registers endpoints

            app.UseRateLimiter();

            app.UseSwaggerAndUI(); // Use Swagger and Swagger UI after all other middleware

            app.MapHealthChecks("/health");

            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var dbContext = services.GetRequiredService<MusicDbContext>();
                    dbContext.Database.EnsureCreated(); // Ensures tables exist
                    dbContext.Database.Migrate(); // Apply pending migrations
                    SeedData.Initialize(scope.ServiceProvider, dbContext);  // Then seed data
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database migration failed. Application will now exit.");
            }

            return app;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed.");
            throw;
        }
    }
}
