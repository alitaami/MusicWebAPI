using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Entities.Subscription_Models
{
    public class UserSubscription
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? PaymentReference { get; set; } = null;
        public bool IsVerified { get; set; }

        public SubscriptionPlan Plan { get; set; }
    }

}
