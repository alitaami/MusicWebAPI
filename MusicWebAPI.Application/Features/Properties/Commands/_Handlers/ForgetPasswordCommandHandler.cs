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
