using MediatR;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.API.Base;
using System.Web.WebPages.Html;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;
using MusicWebAPI.Application.Features.Properties.Commands.Handlers;
using MusicWebAPI.Application.Features.Properties.Commands;
using Microsoft.AspNetCore.Authorization;
using MusicWebAPI.Application.Features.Properties.Queries;
using Microsoft.AspNetCore.Http;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.UserViewModel;
using MusicWebAPI.Core.Base;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using MusicWebAPI.Core.Utilities;
namespace MusicWebAPI.API.Endpoints
{
    public class UserEndpoints : ApiResponseBase
    {
        public static void RegisterUserEndpoints(WebApplication app)
        {
            // Register user endpoint
            app.MapPost("/api/register", async (IMediator mediator, RegisterUserCommand command, HttpContext httpContext, [FromQuery] string? returnUrl = null) =>
            {
                var result = await mediator.Send(command);

                result.ReturnUrl = returnUrl;

                return Ok(result);
            })
            .WithName("RegisterUser")
            .Produces<RegisterUserViewModel>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API

            // Login user endpoint
            app.MapPost("/api/login", async (IMediator mediator, LoginUserCommand command, HttpContext httpContext, [FromQuery] string? returnUrl = null) =>
            {
                // getting JWT token
                var result = await mediator.Send(command);

                result.ReturnUrl = returnUrl;

                return Ok(result);

            })
            .WithName("LoginUser")
            .Produces<LoginUserViewModel>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API

            // AddToPlayList endpoint 
            app.MapPost("/api/playlists",
            async (IMediator mediator, AddToPlaylistDTO dto, HttpContext httpContext) =>
            {
                Guid? userId = (Guid?)Tools.GetUserId(httpContext);
                var command = new AddToPlaylistCommand(dto.SongId, (Guid)userId, dto.PlaylistName, dto.PlaylistId);

                await mediator.Send(command);

                return NoContent();
            })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("AddToPlaylist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("User-Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // GetPlayLists endpoint 
            app.MapGet("/api/playlists",
            async (IMediator mediator,
            HttpContext httpContext) =>
            {
                var userId = (Guid)Tools.GetUserId(httpContext);

                var query = new GetPlaylistsQuery((Guid)userId);
                var result = await mediator.Send(query);

                return Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("GetPlaylists")
            .Produces<List<PlaylistViewModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("User-Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // DeletePlaylist endpoint
            app.MapDelete("/api/playlists/{playlistId}",
            async (IMediator mediator, Guid playlistId) =>
            {
                await mediator.Send(new DeletePlayListCommand(playlistId));
                return NoContent();
            })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("DeletePlaylist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("User-Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // DeleteSongFromPlaylist endpoint
            app.MapDelete("/api/playlists/{playlistId}/songs/{songId}",
            async (IMediator mediator, Guid playlistId, Guid songId) =>
            {
                await mediator.Send(new DeletePlayListSongCommand(songId, playlistId));
                return NoContent();
            })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("DeleteSongFromPlaylist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("User-Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // ListenToSong endpoint
            app.MapPut("/api/songs/{songId}/listen",
            async (IMediator mediator, Guid songId, HttpContext httpContext) =>
                {
                    Guid? userId = (Guid?)Tools.GetUserId(httpContext);

                    await mediator.Send(new ListenToSongCommand(songId, (Guid)userId));
                    return Results.NoContent();
                })
                .RequireAuthorization(policy => policy.RequireRole("User"))
                .WithName("ListenToSong")
                .WithTags("User-Songs")
                .RequireRateLimiting("main")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .WithOpenApi();
        }

        #region Common
        private static async Task AppendTokenToCookies(HttpContext httpContext, string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = true, // Set to true in production (requires HTTPS)
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddHours(2) // Match token expiration
            };
            httpContext.Response.Cookies.Append("access_token", token, cookieOptions);
        }
        #endregion
    }
}
