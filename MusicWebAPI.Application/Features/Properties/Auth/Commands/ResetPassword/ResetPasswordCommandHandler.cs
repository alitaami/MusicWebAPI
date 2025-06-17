using MediatR;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Features.Properties.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IServiceManager _serviceManager;
        private readonly ICacheService _cacheService;

        public ResetPasswordCommandHandler(IServiceManager serviceManager, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _serviceManager = serviceManager;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            string cacheKey = $"{Resource.OtpCode_CacheKey}{request.email}";
            var cachedOtp = await _cacheService.GetAsync<string>(cacheKey);

            if (cachedOtp is null || cachedOtp != request.otpCode)
                throw new LogicException(Resource.WrongOtpCodeError);

            await _serviceManager.User.ResetPassword(request.email, request.newPassword);

            await _cacheService.RemoveAsync(cacheKey);
        }
    }
}
