using MediatR;
using MusicWebAPI.Domain.Entities.Subscription_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.DTOs.UserSongsDTO;
using static MusicWebAPI.Application.ViewModels.SongsViewModel;

namespace MusicWebAPI.Application.Features.Properties.Subscription.Queries.GetSubscriptions
{
    public record GetSubscriptionsQuery() : IRequest<List<SubscriptionPlan>>;
}
