using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mappings.CustomMapping
{
    public static class AutoMapperConfiguration
    {
        public static void InitializeAutoMapper(this IServiceCollection services, params Assembly[] assemblies)
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddCustomMappingProfile();
            });
            IMapper mapper = mapperConfig.CreateMapper();
            mapper.ConfigurationProvider.CompileMappings();
            services.AddSingleton(mapper);
        }
    
        public static void AddCustomMappingProfile(this IMapperConfigurationExpression config)
        {
            config.AddCustomMappingProfile(typeof(IHaveCustomMapping).Assembly);
        }
    
        public static void AddCustomMappingProfile(this IMapperConfigurationExpression config, params Assembly[] assemblies)
        {
            var allTypes = assemblies.SelectMany(a => a.ExportedTypes);
    
            var list = allTypes.Where(type => type.IsClass && !type.IsAbstract &&
                type.GetInterfaces().Contains(typeof(IHaveCustomMapping)))
                .Select(type => (IHaveCustomMapping)Activator.CreateInstance(type));
    
            var profile = new CustomMappingProfile(list);
    
            config.AddProfile(profile);
        }
    }
}