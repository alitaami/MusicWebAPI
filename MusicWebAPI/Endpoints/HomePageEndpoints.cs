using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Core;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.FileService;
using Serilog;
using System.Drawing.Printing;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.API.Endpoints
{
    public class HomePageEndpoints : ApiResponseBase
    {
        public static void RegisterHomeEndpoints(WebApplication app)
        {
            app.MapGet("/api/home/songs", async (IMediator mediator, [FromQuery] string? term = null, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1) =>
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

            app.MapGet("/api/home/recommended-songs", async (IMediator mediator, HttpContext httpContext, int count, CancellationToken ct) =>
            {
                var userId = GetUserId(httpContext);
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
        }

        private static object GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("");
            }
            return userId.ToString();
        }
    }
}
