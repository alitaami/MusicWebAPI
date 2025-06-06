using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Application.Features.Properties.Commands;
using MusicWebAPI.Core.Base;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly HttpClient _httpClient;

        public SubscriptionService(IRepositoryManager repositoryManager, HttpClient httpClient)
        {
            _repositoryManager = repositoryManager;
            _httpClient = httpClient;
            Stripe.StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        }

        public async Task<List<SubscriptionPlan>> GetPlans(CancellationToken cancellationToken)
        {
            return await _repositoryManager.Subscription.GetPlans(cancellationToken);
        }

        public async Task<string> Subscribe(Guid planId, Guid userId, CancellationToken cancellationToken, string? callbackBaseUrl = null)
        {
            bool activePlan = await ActivePlanExists(userId, cancellationToken);

            if (activePlan)
                throw new LogicException(Resource.AlreadySubscribedError);

            var plan = await _repositoryManager.Subscription.GetByIdAsync(cancellationToken, planId);

            if (plan == null || !plan.IsActive)
                throw new NotFoundException(Resource.InvalidPlan);

            var domain = callbackBaseUrl?.TrimEnd('/');

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmountDecimal = plan.Price * 100, // Stripe expects amount in cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = plan.Name
                        }
                    },
                    Quantity = 1
                }
            },
                Mode = "payment",
                SuccessUrl = $"{domain}/api/subscriptions/verify?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/payment/cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            var subscription = new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = plan.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(plan.DurationInDays),
                PaymentReference = session.Id,
                IsVerified = false
            };

            await _repositoryManager.UserSubscription.AddAsync(subscription, cancellationToken, saveNow: true);

            // Return URL to Stripe Checkout
            return session.Url!;
        }

        public async Task<bool> VerifyPayment(string sessionId, CancellationToken cancellationToken)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                var subscription = await _repositoryManager
                    .UserSubscription
                    .Get(cancellationToken)
                    .Where(x => x.PaymentReference == sessionId)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                    throw new NotFoundException(Resource.SubscriptionNotFound);

                if (subscription.IsVerified)
                    return true;

                subscription.IsVerified = true;
                await _repositoryManager.SaveChangesAsync(cancellationToken);

                return true;
            }

            return false;
        }

        private async Task<bool> ActivePlanExists(Guid userId, CancellationToken cancellationToken)
        {
            return await _repositoryManager
                .UserSubscription
                .TableNoTracking
                .AnyAsync(x => x.UserId == userId && x.IsVerified && x.EndDate > DateTime.UtcNow);
        }
    }
}
