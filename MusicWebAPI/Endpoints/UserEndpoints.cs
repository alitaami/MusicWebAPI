using MediatR;
using MusicWebAPI.API.Base;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.AddToPlaylist;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeletePlaylist;
using MusicWebAPI.Application.Features.Properties.UserSongs.Queries.GetPlaylists;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.DeletePlaylistSong;
using MusicWebAPI.Application.Features.Properties.UserSongs.Commands.ListenSong;
namespace MusicWebAPI.API.Endpoints
{
    public class UserEndpoints : ApiResponseBase
    {
        public static void RegisterUserEndpoints(WebApplication app)
        {
            // AddToPlayList endpoint 
            app.MapPost("/api/playlists",
            async (IMediator mediator, AddToPlaylistDTO dto, HttpContext httpContext) =>
            {
                Guid? userId = (Guid?)Tools.GetUserId(httpContext);
                Guid? playlistId = Guid.TryParse(dto.PlaylistId, out var parsed) ? parsed : null;

                var command = new AddToPlaylistCommand(dto.SongId, (Guid)userId, dto.PlaylistName, playlistId);

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
                    await mediator.Send(new ListenToSongCommand(songId));
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
