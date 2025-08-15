using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Features.Properties.Songs.Queries.GetPopularSongs;
using MusicWebAPI.Application.Features.Properties.Songs.Queries.GetRecommendedSongs;
using MusicWebAPI.Application.Features.Properties.Songs.Queries.GetSongs;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Infrastructure.External.FileService;
using System.ComponentModel;
using System.Security.Claims;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.API.Endpoints
{ 
    public class SongEndpoints : ApiResponseBase
    {
        public static void RegisterHomeEndpoints(WebApplication app)
        {
            app.MapGet("/api/home/songs", async (IMediator mediator, [FromQuery] string? term = " ", [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1) =>
            {
                var query = new GetSongsQuery(term, pageSize, pageNumber);

                var songs = await mediator.Send(query);

                return Ok(songs);
            })
            .WithName("GetSongs")
            .Produces<PaginatedResult<GetSongsViewModel>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Home")
            .RequireRateLimiting("main")
            .WithOpenApi();

            app.MapGet("/api/home/popular-songs", async (IMediator mediator, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1) =>
            {
                var query = new GetPopularSongsQuery(pageSize, pageNumber);

                var songs = await mediator.Send(query);

                return Ok(songs);
            })
            .WithName("GetPopularSongs")
            .Produces<PaginatedResult<GetSongsViewModel>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Home")
            .RequireRateLimiting("main")
            .WithOpenApi();

            app.MapGet("/api/home/recommended-songs", async (IMediator mediator, HttpContext httpContext, CancellationToken ct, int count = 5) =>
            {
                var userId = Tools.GetUserId(httpContext);
                var query = new GetRecommendedSongsQuery(userId.ToString(), count);

                var songs = await mediator.Send(query);

                return Ok(songs);
            })
            .WithName("RecommnededSongs")
            .Produces<List<GetSongsViewModel>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Home")
            .RequireRateLimiting("main")
            .WithOpenApi();

            app.MapGet("/api/home/stream-song/{objectId}", async (
               [Description("objectId is made of: {musicId}.mp3")]
               string objectId,
               HttpContext httpContext,
               FileStorageService fileStorageService,
               ICacheService cacheService,
               [Description("Optional HTTP Range header (e.g. 'bytes=0-1023') for partial file streaming")]
               [FromQuery] string? range = null) =>
            {
                try
                {
                    var rangeHeader = range ?? httpContext.Request.Headers["Range"].ToString();
                    var fileStreamResult = await fileStorageService.GetFileStream(objectId, rangeHeader);

                    if (fileStreamResult == null)
                        return NotFound(Resource.FileNotExists);

                    var response = httpContext.Response;

                    response.ContentType = fileStreamResult.ContentType ?? "audio/mpeg";
                    response.Headers["Accept-Ranges"] = "bytes";
                    response.Headers["Content-Disposition"] = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline")
                    {
                        FileNameStar = objectId
                    }.ToString();

                    if (fileStreamResult.IsPartial)
                    {
                        response.StatusCode = StatusCodes.Status206PartialContent;
                        response.Headers["Content-Range"] = fileStreamResult.ContentRange!;
                    }
                    else
                    {
                        response.StatusCode = StatusCodes.Status200OK;
                    }

                    await fileStreamResult.Stream.CopyToAsync(response.Body);

                    await UpdateListenCountAsync(httpContext, cacheService);

                    return Results.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in streaming file: {ex.Message}");

                    return InternalServerError("");
                }
            })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .AddEndpointFilter<LimitStreamingAttribute>()
            .WithName("StreamSong")
            .Produces(StatusCodes.Status206PartialContent)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status416RangeNotSatisfiable)
            .RequireRateLimiting("main")
            .WithTags("Home");
        }

        #region Common
        private static async Task UpdateListenCountAsync(HttpContext httpContext, ICacheService cacheService)
        {
            int count = 0;
            var userId = httpContext.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return;

            var now = DateTime.UtcNow;
            var windowGroup = now.Hour < 12 ? "1" : "2";
            var keyPattern = Resource.ListenCountCacheKey;
            var cacheKey = string.Format(keyPattern, userId, now.ToString("yyyyMMdd"), windowGroup);

            count = await cacheService.GetAsync<int>(cacheKey);
            count++;

            await cacheService.SetAsync(cacheKey, count, 720); // 720 minutes = 12h
        }

        #endregion
    }
}
