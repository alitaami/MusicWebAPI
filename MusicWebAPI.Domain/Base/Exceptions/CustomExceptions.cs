using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Base.Exceptions
{
    public class CustomExceptions
    {
        public class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message) { }
            public int StatusCode => 404;
        }

        public class BadRequestException : Exception
        {
            public BadRequestException(string message) : base(message) { }
            public int StatusCode => 400;
        }

        public class LogicException : Exception
        {
            public LogicException(string message) : base(message) { }
            public int StatusCode => 422;
        }

        public class InternalServerErrorException : Exception
        {
            public InternalServerErrorException(string message) : base(message) { }
            public int StatusCode => 500;
        }

        public class UnauthorizedException : Exception
        {
            public UnauthorizedException(string message) : base(message) { }
            public int StatusCode => 401;
        }

        // Update the ValidationException to accept a list of validation errors
        public class ValidationException : Exception
        {
            public ValidationException(List<string> errors)
                : base("Validation errors occurred.")
            {
                Errors = errors;
            }

            public List<string> Errors { get; }
            public int StatusCode => 422; // Unprocessable Entity
        }
    }
}
