using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Repositories
{
    public interface IUserSubscriptionRepository : IRepository<UserSubscription>
    {
    }
}
