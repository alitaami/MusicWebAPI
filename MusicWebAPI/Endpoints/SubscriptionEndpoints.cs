using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MusicWebAPI.API.Base;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Commands;
using MusicWebAPI.Application.Features.Properties.Queries;
using MusicWebAPI.Core;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.External.FileService;
using MusicWebAPI.Domain.Interfaces;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.FileService;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Net.Http;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.SubscribeDTOs;
using static MusicWebAPI.Application.ViewModels.GatewayDTO;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;

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
                var userId = Tools.GetUserId(httpContext);
                var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                var command = new SubscribeCommand(dto.PlanId, (Guid)userId, baseUrl);

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
