using Configuration.Swagger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MusicWebAPI.API.Endpoints;
using MusicWebAPI.API.Middlewares;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Infrastructure.Caching;
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

            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<ChatHub>("/chathub");

            app.MapControllers();

            // Register the Minimal API routes
            UserEndpoints.RegisterUserEndpoints(app); // Call the method that registers UserEndpoints
            HomePageEndpoints.RegisterHomeEndpoints(app); // Call the method that registers HomeEndpoints

            app.UseRateLimiter();

            app.UseSwaggerAndUI(); // Use Swagger and Swagger UI 

            app.MapHealthChecks("/health");

            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                  
                    //#region Redis Initialization
                    //var cache = services.GetRequiredService<IDistributedCache>();
                    //var settings = services.GetRequiredService<IOptions<RedisSettings>>();
                    //CacheService.Initialize(cache, settings);
                    //#endregion
                    
                    #region SeedData Initialization
                    var dbContext = services.GetRequiredService<MusicDbContext>();
                    dbContext.Database.EnsureCreated(); // Ensures tables exist
                    dbContext.Database.Migrate(); // Apply pending migrations
                    SeedData.Initialize(scope.ServiceProvider, dbContext);  // Then seed data
                    #endregion


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
