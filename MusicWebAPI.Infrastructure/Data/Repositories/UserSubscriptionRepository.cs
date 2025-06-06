using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Repositories
{
    public class UserSubscriptionRepository : Repository<UserSubscription>, IUserSubscriptionRepository
    {
        public UserSubscriptionRepository(MusicDbContext context) : base(context)
        {
        }
         
    }
}
