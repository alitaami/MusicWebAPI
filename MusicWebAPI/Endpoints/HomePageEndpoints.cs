using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Domain.Entities;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

namespace MusicWebAPI.API.Endpoints
{
    public class HomePageEndpoints : ApiResponseBase
    {
        public static void RegisterHomeEndpoints(WebApplication app)
        {
            app.MapGet("/api/home/songs", async (IMediator mediator, [FromQuery] string term = null, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1) =>
            {
                var query = new GetSongsQuery(term, pageSize, pageNumber);
                var user = await mediator.Send(query);

                return Ok(user);
            })
            .WithName("GetSongs")
            .Produces<GetSongsViewModel>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Home")
            .RequireRateLimiting("main")
            .WithOpenApi();
        }
    }
}
