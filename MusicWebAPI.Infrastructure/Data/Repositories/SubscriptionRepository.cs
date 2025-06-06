using Microsoft.EntityFrameworkCore;
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
    public class SubscriptionRepository : Repository<SubscriptionPlan>, ISubscriptionRepository
    {
        public SubscriptionRepository(MusicDbContext context) : base(context)
        {
        }

        public async Task<List<SubscriptionPlan>> GetPlans(CancellationToken cancellationToken)
        {
            return await TableNoTracking
             .Where(s => s.IsActive)
             .ToListAsync(cancellationToken);
        }
    }
}
