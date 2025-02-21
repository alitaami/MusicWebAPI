using Common.Utilities;
using Configuration.Swagger;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using MusicWebAPI.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Infrastructure.Data.Repositories;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Application.Services.Base;
using MusicWebAPI.Domain.Interfaces.Services.Base;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        // Initialize Serilog with configuration
        var logger = new LoggerConfiguration()
            .WriteTo.Console() // Logs to console
            .WriteTo.File("logs/musicwebapi_log.txt", rollingInterval: RollingInterval.Day) // Logs to a file
            .WriteTo.Seq("http://localhost:5341") // Optional: Seq for structured logging (Web UI Panel)
            .CreateLogger();

        // Set the logger globally for the application
        builder.Logging.ClearProviders(); // Remove other logging providers
        builder.Logging.AddSerilog(logger); // Add Serilog as the logging provider

        try
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            var configuration = builder.Configuration;

            ConfigLogging(builder);

            AddAppDbContext(builder, configuration);

            AddMvcAndJsonOptions(builder);

            AddMinimalMvc(builder);

            AddAppServices(builder);

            AddSwagger(builder);

            ApiRateLimiter(builder);


            return builder;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occurred while configuring services");
            throw;
        }
    }
    private static void AddAppDbContext(WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddDbContext<MusicDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("MusicDbConnection"));
        });
    }
    private static void AddAppServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
        builder.Services.AddScoped<IServiceManager, ServiceManager>();

       // builder.Services.AddAutoMapper(typeof(WebApplication));

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.Configure<IISServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
    }

    private static void AddMvcAndJsonOptions(WebApplicationBuilder builder)
    {
        builder.Services
                         .AddControllers()
                         .AddJsonOptions(options =>
                         {
                             options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                         })
                         .AddNewtonsoftJson(options =>
                         {
                             options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                             options.SerializerSettings.Converters.Add(new StringEnumConverter());
                             options.SerializerSettings.Culture = new CultureInfo("en");
                             options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                             options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                             options.SerializerSettings.DateParseHandling = DateParseHandling.DateTime;
                             options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                             options.SerializerSettings.ContractResolver = new DefaultContractResolver
                             {
                                 NamingStrategy = new CamelCaseNamingStrategy()
                             };
                             options.AllowInputFormatterExceptionMessages = true;
                         });

    }

    public static void AddMinimalMvc(WebApplicationBuilder builder)
    {
        //https://github.com/aspnet/AspNetCore/blob/0303c9e90b5b48b309a78c2ec9911db1812e6bf3/src/Mvc/Mvc/src/MvcServiceCollectionExtensions.cs
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(new AuthorizeFilter()); //Apply AuthorizeFilter as global filter to all actions

            //Like [ValidateAntiforgeryToken] attribute but dose not validatie for GET and HEAD http method
            //You can ingore validate by using [IgnoreAntiforgeryToken] attribute
            //Use this filter when use cookie 
            //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

            //options.UseYeKeModelBinder();
        }).AddNewtonsoftJson(option =>
        {
            option.SerializerSettings.Converters.Add(new StringEnumConverter());
            option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //option.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            //option.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        });
        builder.Services.AddSwaggerGenNewtonsoftSupport();

        #region Old way (We don't need this from ASP.NET Core 3.0 onwards)
        ////https://github.com/aspnet/Mvc/blob/release/2.2/src/Microsoft.AspNetCore.Mvc/MvcServiceCollectionExtensions.cs
        //services.AddMvcCore(options =>
        //{
        //    options.Filters.Add(new AuthorizeFilter());

        //    //Like [ValidateAntiforgeryToken] attribute but dose not validatie for GET and HEAD http method
        //    //You can ingore validate by using [IgnoreAntiforgeryToken] attribute
        //    //Use this filter when use cookie 
        //    //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

        //    //options.UseYeKeModelBinder();
        //})
        //.AddApiExplorer()
        //.AddAuthorization()
        //.AddFormatterMappings()
        //.AddDataAnnotations()
        //.AddJsonOptions(option =>
        //{
        //    //option.JsonSerializerOptions
        //})
        //.AddNewtonsoftJson(/*option =>
        //{
        //    option.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        //    option.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        //}*/)

        ////Microsoft.AspNetCore.Mvc.Formatters.Json
        ////.AddJsonFormatters(/*options =>
        ////{
        ////    options.Formatting = Newtonsoft.Json.Formatting.Indented;
        ////    options.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        ////}*/)

        //.AddCors()
        //.SetCompatibilityVersion(CompatibilityVersion.Latest); //.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        #endregion
    }

    private static void AddSwagger(WebApplicationBuilder builder)
    {
        Assert.NotNull(builder.Services, nameof(builder.Services));

        //Add services to use Example Filters in swagger
        //services.AddSwaggerExamples();
        //Add services and configuration to use swagger
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlDocPath = Path.Combine(AppContext.BaseDirectory, "MyApi.xml");
            //show controller XML comments like summary
            options.IncludeXmlComments(xmlDocPath, true);
            //options.OperationFilter<FormFileSwaggerFilter>();
            //options.EnableAnnotations();
            options.UseInlineDefinitionsForEnums();
            //options.DescribeAllParametersInCamelCase();
            //options.DescribeStringEnumsInCamelCase();
            //options.UseReferencedDefinitionsForEnums();
            //options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            //options.IgnoreObsoleteActions();
            //options.IgnoreObsoleteProperties();
            //options.CustomSchemaIds(type => type.FullName);

            options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API V1" });
            //options.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API V2" });

            #region Filters
            ////Enable to use [SwaggerRequestExample] & [SwaggerResponseExample]
            //options.ExampleFilters();

            ////Adds an Upload button to endpoints which have [AddSwaggerFileUploadButton]
            options.OperationFilter<FileUploadOperation>(); // Add this line to enable file upload
                                                            ////Set summary of action if not already set
            options.OperationFilter<ApplySummariesOperationFilter>();

            //#region Add UnAuthorized to Response
            ////Add 401 response and security requirements (Lock icon) to actions that need authorization
            options.OperationFilter<UnauthorizedResponsesOperationFilter>(false, "Bearer");
            //#endregion

            #region security for swagger

            //        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //        {
            //            In = ParameterLocation.Header,
            //            Description = "Please enter a valid token",
            //            Name = "Authorization",
            //            Type = SecuritySchemeType.Http,
            //            BearerFormat = "JWT",
            //            Scheme = "Bearer"
            //        });
            //        options.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type=ReferenceType.SecurityScheme,
            //                Id="Bearer"
            //            }
            //        },
            //        new string[]{}
            //    }
            //});

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri("https://localhost:7076/api/Account/Login"),
                        Scopes = new Dictionary<string, string>
            {
                {"read", "Read access to protected resources."},
                {"write", "Write access to protected resources."},
            }
                    }
                }
            });

            #endregion

            //#region Add Jwt Authentication
            //Add Lockout icon on top of swagger ui page to authenticate

            //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            //    Name = "Authorization",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.Http,
            //    Scheme = "bearer"
            //});

            ////options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
            ////{
            ////    {"Bearer", new string[] { }}
            ////});
            ///
            //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    Type = SecuritySchemeType.OAuth2,
            //    Flows = new OpenApiOAuthFlows
            //    {
            //        Implicit = new OpenApiOAuthFlow
            //        {
            //            AuthorizationUrl = new Uri("https://localhost:7188/api/v1/User/login"),
            //            Scopes = new Dictionary<string, string>
            //    {
            //        {"read", "Read access to protected resources."},
            //        {"write", "Write access to protected resources."},
            //    }
            //        }
            //    }
            //});
            #endregion

            #region Versioning
            //// Remove version parameter from all Operations
            //options.OperationFilter<RemoveVersionParameters>();

            //////set version "api/v{version}/[controller]" from current swagger doc verion
            //options.DocumentFilter<SetVersionInPaths>();

            //////Seperate and categorize end-points by doc version
            //options.DocInclusionPredicate((docName, apiDesc) =>
            //{
            //    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

            //    var versions = methodInfo.DeclaringType
            //        .GetCustomAttributes<ApiVersionAttribute>(true)
            //        .SelectMany(attr => attr.Versions);

            //    return versions.Any(v => $"v{v.ToString()}" == docName);
            //});
            #endregion

            //If use FluentValidation then must be use this package to show validation in swagger (MicroElements.Swashbuckle.FluentValidation)
            //options.AddFluentValidationRules();

        });
    }


    private static void ConfigLogging(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog(); // Use Serilog as the global logging provider
    }
    private static void ApiRateLimiter(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            // Set the status code to be returned when the rate limit is exceeded
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Add a rate limiting policy named "test"
            options.AddPolicy("test", httpContext =>
                // Get a fixed window rate limiter based on the client's IP address   
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                    factory: x => new FixedWindowRateLimiterOptions
                    {
                        // Set the maximum number of permits allowed within the window
                        PermitLimit = 5,
                        // Set the duration of the window 
                        Window = TimeSpan.FromSeconds(5)
                    }));
        });
    }
}
