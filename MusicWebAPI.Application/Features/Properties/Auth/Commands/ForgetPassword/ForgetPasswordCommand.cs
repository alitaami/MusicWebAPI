﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Features.Properties.Auth.Commands.ForgetPassword
{
    public record ForgetPasswordCommand(string email) : IRequest;
}
