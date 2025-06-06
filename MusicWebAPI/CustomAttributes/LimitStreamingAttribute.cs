using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MusicWebAPI.Core.Base;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MusicWebAPI.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core.Resources;

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
        var dbContext = httpContext.RequestServices.GetService<MusicDbContext>();

        if (cacheService == null || dbContext == null)
        {
            var errorResult = new ApiResult<string>(Resource.GeneralUnhandledError, 500, isSuccess: false);
            return Results.Json(errorResult, statusCode: 500);
        }

        var now = DateTime.UtcNow;

        var activeSubscription = await dbContext.UserSubscriptions
            .Include(us => us.Plan)
            .FirstOrDefaultAsync(us =>
                us.UserId == userId &&
                us.IsVerified &&
                us.StartDate <= now &&
                us.EndDate >= now &&
                us.Plan.IsActive);

        if (activeSubscription != null)
        {
            // User has an active subscription plan - skip limit check
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
