using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Entities.Subscription_Models
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; }
        public int DurationInDays { get; set; }
        public decimal Price { get; set; } // in Toman
        public bool IsActive { get; set; }
    } 
}
