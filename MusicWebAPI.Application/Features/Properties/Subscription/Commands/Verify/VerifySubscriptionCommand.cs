﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Properties.Subscription.Commands.Verify
{
    public record VerifySubscriptionCommand(string session_id) : IRequest<bool>;
}
