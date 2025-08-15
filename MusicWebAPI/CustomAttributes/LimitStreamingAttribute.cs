using MusicWebAPI.Core.Base;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.External;
using MusicWebAPI.Infrastructure.Data.Context;
using System.Security.Claims;
using System.Text.Json;
using static MusicWebAPI.Application.ViewModels.GatewayViewModel;

[AttributeUsage(AttributeTargets.Method)]
public class LimitStreamingAttribute : Attribute, IEndpointFilter
{
    private const int MaxStreams = 10;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var userIdString = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            var errorResult = new ApiResult<string>(Resource.UnAuthorizedError, 401);
            return Results.Json(errorResult, statusCode: 401);
        }

        var cacheService = httpContext.RequestServices.GetService<ICacheService>();
        var rabbitMqService = httpContext.RequestServices.GetService<IRabbitMqService>();
        var dbContext = httpContext.RequestServices.GetService<MusicDbContext>();

        if (cacheService == null || rabbitMqService == null || dbContext == null)
        {
            var errorResult = new ApiResult<string>(Resource.GeneralUnhandledError, 500, isSuccess: false);
            return Results.Json(errorResult, statusCode: 500);
        }

        // *** it`s just for learning the main purpose of message brokers! we can have a separate background job to consume and cache the value in Redis ***
        // Consume subscription events from RabbitMQ & update cache
        rabbitMqService.Consume("SubscriptionPurchased", async message =>
        {
            var payload = JsonSerializer.Deserialize<SubscriptionEventViewModel>(message);
            if (payload != null)
            {
                var key = $"Subscription:{payload.UserId}";
                var expiration = payload.EndDate - DateTime.UtcNow;

                if (expiration < TimeSpan.Zero)
                {
                    expiration = TimeSpan.FromMinutes(5); // safety fallback
                }

                await cacheService.SetAsync(key, value: true, expiration.Minutes);
            }
        });

        var now = DateTime.UtcNow;

        // Check if user has active subscription 
        var subscriptionKey = $"Subscription:{userId}";
        var hasActiveSubscription = await cacheService.GetAsync<bool>(subscriptionKey);

        // User has an active subscription plan, So skip limit check
        if (hasActiveSubscription)
        {
            return await next(context);
        }

        // No active subscription, enforce stream limit
        var windowGroup = now.Hour < 12 ? "1" : "2";
        var keyPattern = Resource.ListenCountCacheKey;
        var cacheKey = string.Format(keyPattern, userId, now.ToString("yyyyMMdd"), windowGroup);

        var streamCount = await cacheService.GetAsync<int>(cacheKey);

        if (streamCount >= MaxStreams)
        {
            var errorResult = new ApiResult<string>(
                Resource.SongStreamLimitError,
                422,
                isSuccess: false);

            return Results.Json(errorResult, statusCode: 422);
        }

        return await next(context);
    }
}
