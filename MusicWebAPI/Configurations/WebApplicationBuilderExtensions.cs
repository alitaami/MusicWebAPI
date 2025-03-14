using Common.Utilities;
using Configuration.Swagger;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using MusicWebAPI.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Infrastructure.Data.Repositories;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Domain.Interfaces.Services.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MusicWebAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MusicWebAPI.Application;
using Serilog.Sinks.Elasticsearch;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;
using DotNetEnv;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        Env.Load();
        var configuration = builder.Configuration;

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

        try
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            AddAppDbContext(builder, configuration);

            AddMvcAndJsonOptions(builder);

            AddMinimalMvc(builder);

            AddAppServices(builder);

            AddCors(builder);

            AddJwtAuthentication(builder, configuration);

            AddSwagger(builder);

            ApiRateLimiter(builder);

            builder.Services.AddApplicationServices(); //Adding configuration of 'Application layer'

            return builder;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while configuring services");
            throw;
        }
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
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
    }

    private static void AddAppServices(WebApplicationBuilder builder)
    {
        // Register Repositories , Services and LoggerManager
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
        builder.Services.AddScoped<IServiceManager, ServiceManager>();

        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<MusicDbContext>()
            .AddDefaultTokenProviders();

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
