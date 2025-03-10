using MediatR;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.API.Base;
using System.Web.WebPages.Html;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;
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
            .Produces<User>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API

            // Login user endpoint
            app.MapPost("/api/login", async (IMediator mediator, LoginUserCommand command) =>
            {
                // getting JWT token
                var token = await mediator.Send(command);

                return Ok(token);

            })
            .WithName("LoginUser")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API
        }
    }
}
