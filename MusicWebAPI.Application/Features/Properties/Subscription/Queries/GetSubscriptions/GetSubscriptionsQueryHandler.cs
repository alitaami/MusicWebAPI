using MediatR;
using MusicWebAPI.Application.Features.Properties.Subscription.Queries.GetSubscriptions;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.HomeViewModel;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.Queries.Handlers
{
    public class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, List<SubscriptionPlan>>
    {
        private readonly IServiceManager _serviceManager;

        public GetSubscriptionsQueryHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<List<SubscriptionPlan>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
        {
            return await _serviceManager.Subscription.GetPlans(cancellationToken);
        }
    }
}
