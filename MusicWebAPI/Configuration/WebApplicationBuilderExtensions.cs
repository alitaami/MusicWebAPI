using Common.Utilities;
using Configuration.Swagger;
using DotNetEnv;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using MusicWebAPI.Application;
using MusicWebAPI.Core.Utilities;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.External.FileService;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using MusicWebAPI.Domain.Interfaces.Services.External;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using MusicWebAPI.Infrastructure.External.Caching;
using MusicWebAPI.Infrastructure.External.FileService;
using MusicWebAPI.Infrastructure.External.Outbox;
using MusicWebAPI.Infrastructure.External.RabbitMq;
using MusicWebAPI.Infrastructure.External.RedisLock;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using StackExchange.Redis;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using static System.Net.WebRequestMethods;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        #region Logging Configuration

        // Enable Serilog self-logging (optional for debugging Serilog itself)
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        // Retrieve values from the .env file
        var logFilePath = Environment.GetEnvironmentVariable("LOG_FILE_PATH");
        var elasticsearchUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
        var indexFormat = Environment.GetEnvironmentVariable("INDEX_FORMAT");

        // Configure the logger entirely in code
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticsearchUri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = indexFormat,
                FailureCallback = HandleElasticsearchFailure,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.RaiseCallback,
                CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true)
            })
            .CreateLogger();

        // Use Serilog as the logging provider
        builder.Host.UseSerilog(logger);
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        #endregion

        try
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            AddAppDbContext(builder, configuration);

            AddMvcAndJsonOptions(builder);

            AddMinimalMvc(builder);

            AddAppServices(builder);

            AddHangfire(builder, configuration);

            AddRedis(builder, configuration);

            AddCors(builder);

            AddJwtAuthentication(builder, configuration);

            AddSwagger(builder);

            AddRateLimiter(builder);

            AddYarp(builder);

            builder.Services.AddApplicationServices(); //Adding configuration of 'Application layer'

            return builder;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while configuring services");
            throw;
        }
    }

    public static void AddYarp(WebApplicationBuilder builder)
    {
        //YARP reverse proxy
        builder.Services.AddReverseProxy()
               .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    }
    public static void AddHangfire(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MusicDbConnection");

        builder.Services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(connectionString);
            }));

        builder.Services.AddHangfireServer();
    }

    public static void AddRedis(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("REDIS_CONNECTION_STRING environment variable is not set.");
        }

        // Existing cache registration
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "MusicWebAPI";
        });

        // shared ConnectionMultiplexer
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // RedLock factory
        builder.Services.AddSingleton<IDistributedLockFactory>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();

            var multiplexers = new List<RedLockMultiplexer>
            {
            new RedLockMultiplexer(multiplexer)
            };

            return RedLockFactory.Create(multiplexers);
        }); 
    }

    private static void HandleElasticsearchFailure(LogEvent logEvent, Exception ex)
    {
        Console.WriteLine($"Unable to log event: {ex.Message} | Log: {logEvent.RenderMessage()}");
    }

    private static void AddJwtAuthentication(WebApplicationBuilder builder, IConfiguration configuration)
    {
        // Read JWT settings from configuration
        var jwtSection = configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        // Configure JWT authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            // enables reading token from Cookie named "access_token"
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["access_token"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();
    }
    private static void AddAppDbContext(WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddDbContext<MusicDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("MusicDbConnection"))
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information);
        });
    }
    private static void AddCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .WithOrigins("http://localhost:4200", "http://localhost:8080")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private static void AddAppServices(WebApplicationBuilder builder)
    {
        #region SignalR
        builder.Services.AddSignalR();
        #endregion

        #region Minio

        // Configure MinIO Client using environment variables

        builder.Services.AddSingleton(
    new MinioClient()
        .WithEndpoint(Environment.GetEnvironmentVariable("MINIO_ENDPOINT"))
        .WithCredentials(
            Environment.GetEnvironmentVariable("MINIO_ROOT_USER"),
            Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD")
        )
        .Build()
        );

        #endregion

        // Register Repositories , Services and LoggerManager
        builder.Services.AddSingleton<FileStorageService>();
        builder.Services.AddSingleton<ICacheService, CacheService>();
        builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
        builder.Services.AddScoped<IOutboxService, OutboxService>();
        builder.Services.AddTransient<OutboxProcessor>();
        builder.Services.AddSingleton<IRedisLockFactory, RedisLockFactory>();

        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
        builder.Services.AddScoped<IServiceManager, ServiceManager>();

        #region Identity
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<MusicDbContext>()
            .AddDefaultTokenProviders();
        #endregion

        builder.Services.AddHttpClient();

        // Enable API Explorer for Swagger
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddHealthChecks();

        // IIS Configuration (if needed)
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
                             options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                             options.SerializerSettings.Converters.Add(new StringEnumConverter());
                             options.SerializerSettings.Culture = new CultureInfo("en");
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

            //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    Type = SecuritySchemeType.OAuth2,
            //    Flows = new OpenApiOAuthFlows
            //    {
            //        Password = new OpenApiOAuthFlow
            //        {
            //            TokenUrl = new Uri("https://localhost:7002/api/Login"),
            //            Scopes = new Dictionary<string, string>
            //{
            //    {"read", "Read access to protected resources."},
            //    {"write", "Write access to protected resources."},
            //}
            //        }
            //    }
            //});

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


            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
});


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
    private static void AddRateLimiter(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            // Set the status code to be returned when the rate limit is exceeded
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Add a rate limiting policy named "test"
            options.AddPolicy("main", httpContext =>
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
