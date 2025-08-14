using Configuration.Swagger;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MusicWebAPI.API.Endpoints;
using MusicWebAPI.API.Middlewares;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Interfaces;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Infrastructure.Caching;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Outbox;
using Serilog;
using System.Net;
using static MusicWebAPI.Core.Utilities.Tools;

public static class WebApplicationExtensions
{
    public static WebApplication Configure(this WebApplication app)
    {
        try
        {
            #region Middlewares
            UseMiddlewares(app);
            #endregion

            app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
            app.MapGet("/chat", () => Results.Redirect("/Content/Views/chat.html"));

            app.UseHttpsRedirection();
            app.UseHsts();
            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            #region SeedData
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;

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
            #endregion

            app.MapHub<ChatHub>("/chathub");
            app.MapControllers();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }

                //Authorization = new[] { new RoleBasedAuthorizationFilter("SuperUser") }
            });

            #region Scheduling Jobs
            JobScheduler.ScheduleJobs();
            #endregion

            // Register the Minimal API routes
            #region MinimalApi`s
            RegisterMinimalApis(app);
            #endregion

            app.UseRateLimiter();

            app.UseSwaggerAndUI(); // Use Swagger and Swagger UI 

            app.MapHealthChecks("/health");

            return app;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed.");
            throw;
        }
    }

    #region ExtensionMethods
    private static class JobScheduler
    {
        public static void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate<IRecommendationService>(
            "ml-training-every-3-hours",
            x => x.Train(CancellationToken.None),
            "0 */3 * * *",
            TimeZoneInfo.Local
            );

            RecurringJob.AddOrUpdate<OutboxProcessor>("outbox-processor",
            p => p.ProcessPendingAsync(),
            Cron.Minutely);
        }
    }
    private static void UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<LoggingMiddleware>();
    }

    private static void RegisterMinimalApis(this WebApplication app)
    {
        AuthEndpoints.RegisterAuthEndpoints(app); // Call the method that registers AuthEndpoints
        UserEndpoints.RegisterUserEndpoints(app); // Call the method that registers UserEndpoints
        SongEndpoints.RegisterHomeEndpoints(app); // Call the method that registers HomeEndpoints
        SubscriptionEndpoints.RegisterSubscriptionEndpoints(app); // Call the method that registers SubscriptionEndpoints
    }

    public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Always allow access to the dashboard
            return true;
        }
    }

    #endregion
}
