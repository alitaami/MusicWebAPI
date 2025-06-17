using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Features.Properties.Subscription.Commands.Subscribe;
using MusicWebAPI.Application.Features.Properties.Subscription.Commands.Verify;
using MusicWebAPI.Application.Features.Properties.Subscription.Queries.GetSubscriptions;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using static MusicWebAPI.Application.DTOs.SubscribeDTOs;
using static MusicWebAPI.Application.ViewModels.GatewayDTO;

namespace MusicWebAPI.API.Endpoints
{
    public class SubscriptionEndpoints : ApiResponseBase
    {
        public static void RegisterSubscriptionEndpoints(WebApplication app)
        {
            // Get all subscription plans (offers)
            app.MapGet("/api/subscriptions/plans", async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetSubscriptionsQuery();
                var plans = await mediator.Send(query, cancellationToken);

                return Ok(plans);
            })
            .WithName("GetSubscriptionPlans")
            .Produces<List<SubscriptionPlan>>(StatusCodes.Status200OK)
            .WithTags("Subscription")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // Subscribe to a plan  
            app.MapPost("/api/subscriptions/subscribe", async (IMediator mediator, [FromBody] SubscribeDTO dto, HttpContext httpContext, CancellationToken cancellationToken) =>
            {
                var userId = (Guid)Tools.GetUserId(httpContext);
                var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                var command = new SubscribeCommand(dto.PlanId, userId, baseUrl);

                var paymentUrl = await mediator.Send(command, cancellationToken);

                return Ok(new RedirectToGatewayResponseDTO
                {
                    CheckoutUrl = paymentUrl
                });
            })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("SubscribeToPlan")
            .Produces<RedirectToGatewayResponseDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("Subscription")
            .RequireRateLimiting("main")
            .WithOpenApi();

            // Verify subscription payment
            app.MapGet("/api/subscriptions/verify", async (IMediator mediator, [FromQuery] string session_id, CancellationToken ct) =>
            {
                var command = new VerifySubscriptionCommand(session_id);

                var result = await mediator.Send(command, ct);

                return result
                    ? Ok("Payment successful. Subscription activated.")
                    : BadRequest("Payment failed.");
            })
            .WithName("VerifySubscription")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags("Subscription")
            .RequireRateLimiting("main")
            .WithOpenApi();
        } 
    }
}
