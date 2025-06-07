using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Features.Properties.Commands.Auth;
using static MusicWebAPI.Application.ViewModels.UserViewModel;

namespace MusicWebAPI.API.Endpoints
{
    public class AuthEndpoints : ApiResponseBase
    {
        public static void RegisterAuthEndpoints(WebApplication app)
        { // Getting client_id from .env file to use in Google login
            app.MapGet("/api/auth/google/client_id", () =>
            {
                var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
                return Ok(clientId);
            })
            .WithName("Google-ClientId")
            .RequireRateLimiting("main")
            .ExcludeFromDescription();

            // Google login endpoint
            app.MapPost("/api/auth/google-login", async (IMediator mediator, GoogleLoginCommand command) =>
            {
                var result = await mediator.Send(command);

                return Ok(result);
            })
            .WithName("Google-Login")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .WithTags("Auth")
            .RequireRateLimiting("main")
            .WithOpenApi();

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
            .WithTags("Auth")
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
            .WithTags("Auth")
            .RequireRateLimiting("main")
            .WithOpenApi(); // This enables Swagger for Minimal API
        }
    }
}
