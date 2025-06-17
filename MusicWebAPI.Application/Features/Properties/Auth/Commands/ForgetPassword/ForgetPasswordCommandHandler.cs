using MediatR;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.Base;

namespace MusicWebAPI.Application.Features.Properties.Auth.Commands.ForgetPassword
{
    public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand>
    {
        private readonly IServiceManager _serviceManager;
        private readonly ICacheService _cacheService;

        public ForgetPasswordCommandHandler(IServiceManager serviceManager, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _serviceManager = serviceManager;
        }

        public async Task Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            var otp = await _serviceManager.User.ForgetPassword(request.email);
             
            string cacheKey = $"{Resource.OtpCode_CacheKey}{request.email}";

            await _cacheService.SetAsync(cacheKey, otp, minutes: 10, prefix: Resource.OtpCode_CacheKey);
        }
    }
}
