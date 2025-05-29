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
namespace MusicWebAPI.API.Endpoints
{
    public class UserEndpoints : ApiResponseBase
    {
        public static void RegisterUserEndpoints(WebApplication app)
        {
            // Register user endpoint
            app.MapPost("/api/register", async (IMediator mediator, RegisterUserCommand command) =>
            {
                var user = await mediator.Send(command);

                return Ok(user);
            })
            .WithName("RegisterUser")
            .Produces<RegisterUserViewModel>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API

            // Login user endpoint
            app.MapPost("/api/login", async (IMediator mediator, LoginUserCommand command, HttpContext httpContext) =>
            {
                // getting JWT token
                var token = await mediator.Send(command);

                await AppendTokenToCookies(httpContext, token.Token);

                return Ok(token);

            })
            .WithName("LoginUser")
            .Produces<LoginUserViewModel>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API

            // AddToPlayList endpoint 
            app.MapPost("/api/playlists", [Authorize(Roles = "User")]
            async (IMediator mediator, AddToPlaylistDTO dto, HttpContext httpContext) =>
            {
                Guid? userId = (Guid?)GetUserId(httpContext);
                var command = new AddToPlaylistCommand(dto.SongId, (Guid)userId, dto.PlaylistName, dto.PlaylistId);

                await mediator.Send(command);

                return NoContent();
            })
            .WithName("AddToPlaylist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // GetPlayLists endpoint 
            app.MapGet("/api/playlists",
            [Authorize(Roles = "User")]
            async (IMediator mediator,
            HttpContext httpContext) =>
            {
                // Extract user id or info from ClaimsPrincipal (httpContext.User)
                var userId = GetUserId(httpContext);

                var query = new GetPlaylistsQuery((Guid)userId);
                var result = await mediator.Send(query);

                return Ok(result);
            })
            .WithName("GetPlaylists")
            .Produces<List<PlaylistViewModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // DeletePlaylist endpoint
            app.MapDelete("/api/playlists/{playlistId}",
            [Authorize(Roles = "User")]
            async (IMediator mediator, Guid playlistId) =>
            {
                await mediator.Send(new DeletePlayListCommand(playlistId));
                return NoContent();
            })
            .WithName("DeletePlaylist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();


            // DeleteSongFromPlaylist endpoint
            app.MapDelete("/api/playlists/{playlistId}/songs/{songId}",
            [Authorize(Roles = "User")]
            async (IMediator mediator, Guid playlistId, Guid songId) =>
            {
                await mediator.Send(new DeletePlayListSongCommand(songId, playlistId));
                return NoContent();
            })
            .WithName("DeleteSongFromPlaylist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Playlists")
            .RequireRateLimiting("main")
            .WithOpenApi();

        }

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

        private static object GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("");
            }
            return userId;
        }
    }
}
