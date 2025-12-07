using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.External;
using MusicWebAPI.Infrastructure.Outbox;
using Stripe.Checkout;
using System.Text.Json;
using static MusicWebAPI.Application.ViewModels.GatewayViewModel;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IOutboxService _outbox;
        private readonly IRedisLockFactory _lockFactory;

        public SubscriptionService(IRepositoryManager repositoryManager, IOutboxService outbox, IRedisLockFactory lockFactory)
        {
            _repositoryManager = repositoryManager;
            _outbox = outbox;
            _lockFactory = lockFactory;

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

            if (session.PaymentStatus != "paid")
                return false;

            // One lock per Stripe session 
            var resource = $"lock_subscription_session_{sessionId}";
            var expiry = TimeSpan.FromSeconds(30);

            using (var redLock = await _lockFactory.CreateLockAsync(resource, expiry))
            {
                if (!redLock.IsAcquired)
                    throw new LogicException(Resource.SubscriptionRedisLockError);
                 
                var subscription = await _repositoryManager
                    .UserSubscription
                    .Get(cancellationToken)
                    .Where(x => x.PaymentReference == sessionId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (subscription == null)
                    throw new NotFoundException(Resource.SubscriptionNotFound);

                //if already verified, do nothing but still succeed
                if (subscription.IsVerified)
                    return true;

                subscription.IsVerified = true;

                #region Publish Subscription Events in RabbitMQ
                await _outbox.AddAsync(new OutboxMessage
                {
                    Type = "Event:SubscriptionPurchased",
                    Content = JsonSerializer.Serialize(new SubscriptionEventViewModel
                    {
                        UserId = subscription.UserId,
                        PlanId = subscription.PlanId,
                        Timestamp = DateTime.UtcNow
                    }),
                    OccuredDate = DateTime.UtcNow
                });
                #endregion

                await _repositoryManager.SaveChangesAsync(cancellationToken);

                return true;
            }
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
