using FluentValidation;
using Mappings.CustomMapping;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MusicWebAPI.Application.Services;
using MusicWebAPI.Domain.Interfaces.Services.MusicWebAPI.Domain.Interfaces;
using System.Reflection;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Application.Features.Properties.Auth.Commands.Register;
using MusicWebAPI.Infrastructure.External.Logging;

namespace MusicWebAPI.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerManager, LoggerManager>(); 
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddScoped<IUserService, UserService>();

            services.AddAutoMapper(Assembly.GetExecutingAssembly())
                    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RegisterCommand>())
                    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)) // Add Validation Pipeline
                    .InitializeAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
