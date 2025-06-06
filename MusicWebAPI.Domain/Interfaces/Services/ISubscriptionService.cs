using MusicWebAPI.Domain.Entities.Subscription_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlan>> GetPlans(CancellationToken cancellationToken);
        Task<bool> VerifyPayment(string sessionId, CancellationToken cancellationToken);
        Task<string> Subscribe(Guid planId, Guid userId, CancellationToken cancellationToken, string? callbackBaseUrl = null);
    }
}
