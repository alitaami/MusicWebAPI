using MediatR;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Properties.Subscription.Commands.Verify
{
    public class VerifySubscriptionCommandHandler : IRequestHandler<VerifySubscriptionCommand, bool>
    {
        private readonly IServiceManager _serviceManager;

        public VerifySubscriptionCommandHandler(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        public async Task<bool> Handle(VerifySubscriptionCommand request, CancellationToken cancellationToken)
        {
            return await _serviceManager.Subscription.VerifyPayment(request.session_id, cancellationToken);
        }
    }
}
