using MediatR;
using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Application.Commands;
using MusicWebAPI.Application.Features.Properties.Commands.Auth;
using MusicWebAPI.Application.Features.Properties.Commands.UserSongs;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Application.Features.Properties.Commands.Handlers
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
