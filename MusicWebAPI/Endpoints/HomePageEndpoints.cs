using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.External.FileService;
using MusicWebAPI.Domain.Interfaces;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.FileService;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Net.Http;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.API.Endpoints
{
    public class HomePageEndpoints : ApiResponseBase
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


            app.MapGet("/api/home/stream-song/{objectId}", async (
                [Description("objectId is made of: {musicId}.mp3")]
                string objectId,
                HttpContext httpContext,
                FileStorageService fileStorageService,
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
                    return Results.Empty;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return BadRequest(Resource.InvalidRangeHeader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in streaming file: {ex.Message}");

                    return InternalServerError("");
                }
            })
            .WithName("StreamSong")
            .Produces(StatusCodes.Status206PartialContent)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status416RangeNotSatisfiable)
            .WithTags("Home");

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
