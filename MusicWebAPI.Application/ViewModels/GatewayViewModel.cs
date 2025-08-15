using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.ViewModels
{
    public class GatewayViewModel
    {
        public class RedirectToGatewayResponseViewModel
        {
            public string CheckoutUrl { get; set; }
        }

        public class SubscriptionEventViewModel
        {
            public Guid UserId { get; set; }
            public Guid PlanId { get; set; }
            public DateTime Timestamp { get; set; }
            public DateTime EndDate { get; set; } 
        }
    }
}
