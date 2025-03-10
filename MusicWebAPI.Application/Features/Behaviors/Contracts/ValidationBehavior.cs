using FluentValidation;
using MediatR;
using MusicWebAPI.Domain.Interfaces.Services.MusicWebAPI.Domain.Interfaces;
using ValidationException = MusicWebAPI.Domain.Base.Exceptions.CustomExceptions.ValidationException;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILoggerManager _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILoggerManager logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Handling {typeof(TRequest).Name}");

        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count > 0)
            {
                var errorDetails = failures.Select(f => $"Property: {f.PropertyName}, Error: {f.ErrorMessage}").ToList();
                _logger.LogError($"Validation errors in {typeof(TRequest).Name}: {string.Join(" | ", errorDetails)}");

                // Throw ValidationException with errors for client-side handling
                throw new ValidationException(errorDetails);
            }
        }

        var response = await next();
        _logger.LogInfo($"Finished handling {typeof(TRequest).Name}");
        return response;
    }
}
