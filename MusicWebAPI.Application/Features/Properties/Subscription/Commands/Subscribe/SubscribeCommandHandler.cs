using MediatR;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Properties.Subscription.Commands.Subscribe
{
    public class SubscribeCommandHandler : IRequestHandler<SubscribeCommand, string>
    {
        private readonly IServiceManager _serviceManager;

        public SubscribeCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<string> Handle(SubscribeCommand request, CancellationToken cancellationToken)
        {
            return await _serviceManager.Subscription.Subscribe(
                request.PlanId,
                request.UserId,
                cancellationToken,
                request.CallbackBaseUrl
            );
        }
    }
}
