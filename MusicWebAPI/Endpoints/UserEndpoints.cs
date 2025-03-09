using MediatR;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.API.Base;

namespace MusicWebAPI.API.Endpoints
{
    public class UserEndpoints : ApiResponseBase
    {
        public static void RegisterUserEndpoints(WebApplication app)
        {
            // Register user endpoint
            app.MapPost("/api/register", async (IMediator mediator, RegisterUserCommand command) =>
            {
                try
                {
                    var user = await mediator.Send(command);

                    // Return success response wrapped in ApiResult
                    return new UserEndpoints().Ok(user);
                }
                catch (Exception ex)
                { 
                    return new UserEndpoints().BadRequest<User>(ex.Message);
                }
            })
            .WithName("RegisterUser")
            .Produces<User>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .WithOpenApi(); // This enables Swagger for Minimal API

            // Login user endpoint
            app.MapPost("/api/login", async (IMediator mediator, LoginUserCommand command) =>
            {
                try
                {
                    // getting JWT token
                    var token = await mediator.Send(command);

                    // Return success response wrapped in ApiResult
                    return new UserEndpoints().Ok( token );
                }
                catch (Exception ex)
                { 
                    return new UserEndpoints().BadRequest<string>(ex.Message);
                }
            })
            .WithName("LoginUser")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("User")
            .WithOpenApi(); // This enables Swagger for Minimal API
        }
    }
}
