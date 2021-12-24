using System;
using System.Reflection;
using AVStack.IdentityServer.WebApi.Data;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Application;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using AVStack.IdentityServer.WebApi.Services;
using AVStack.MessageBus.Abstraction;
using AVStack.MessageBus.Extensions;
using FluentValidation.AspNetCore;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace AVStack.IdentityServer.WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureWebApi();

            services.ConfigureNpgsql(configuration);

            services.ConfigureCors();
            services.ConfigureIdentity(configuration);
            services.ConfigureIdentityServer(configuration);


            services.ConfigureMessageBus(configuration);
            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));

            services.RegisterModels();
            services.RegisterServices();
        }

        private static void RegisterModels(this IServiceCollection services)
        {
            services.AddScoped<IUser, User>();
        }
        private static void RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IReturnUrlParser, ReturnUrlParser>();
        }

        private static void ConfigureNpgsql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AccountDbContext>(options =>
                options
                    .UseNpgsql(
                        configuration.GetSection("ConnectionStrings")["AVAccount"],
                        option =>
                        {
                            option.UseAdminDatabase("postgres");
                            option.MigrationsAssembly(typeof(AccountDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        }));
                    //.ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore)));

        }
        private static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: !!!IMPORTANT!!!  Bind options from appsettings.json to objects and serve them as singletons
            services.AddIdentity<UserEntity, RoleEntity>(option =>
                {
                    option.User.RequireUniqueEmail = true;
                    option.Password = new PasswordOptions
                    {
                        RequiredLength = 8,
                        RequireDigit = true,
                        RequiredUniqueChars = 2,
                        RequireUppercase = true,
                        RequireLowercase = true
                    };
                    option.Lockout = new LockoutOptions
                    {
                        AllowedForNewUsers = true,
                        MaxFailedAccessAttempts = 5,
                        DefaultLockoutTimeSpan = TimeSpan.FromDays(365),
                    };
                })
                .AddEntityFrameworkStores<AccountDbContext>()
                .AddDefaultTokenProviders();
        }
        private static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddIdentityServer(options =>
                {
                    options.UserInteraction = new UserInteractionOptions
                    {
                        LoginUrl = configuration.GetSection("IdentityServerOptions")["UserInteraction:LoginUrl"],
                        LogoutUrl = configuration.GetSection("IdentityServerOptions")["UserInteraction:LogoutUrl"],
                        ConsentUrl = configuration.GetSection("IdentityServerOptions")["UserInteraction:ConsentUrl"],
                        ErrorUrl = configuration.GetSection("IdentityServerOptions")["UserInteraction:ErrorUrl"],
                        // DeviceVerificationUrl = configuration.GetSection("IdentityServerOptions")["UserInteraction:DeviceVerificationUrl"],
                    };

                    options.Events = new EventsOptions
                    {
                        RaiseErrorEvents = true,
                        RaiseFailureEvents = true,
                        RaiseSuccessEvents = false,
                        RaiseInformationEvents = false
                    };

                })
                .AddAspNetIdentity<UserEntity>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(configuration.GetSection("ConnectionStrings")["AVIdentityServer"], option =>
                        {
                            option.MigrationsAssembly(typeof(AccountDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        });
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(configuration.GetSection("ConnectionStrings")["AVIdentityServer"],  option =>
                        {
                            option.MigrationsAssembly(typeof(AccountDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        });

                    // This enables automatic token cleanup. This is optional.
                    // options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 30;
                })
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential();
        }
        private static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Default", policy =>
                {
                    policy
                        //.WithOrigins("http://localhost:4200", "http://localhost:4201", "https://localhost:5005")
                        .SetIsOriginAllowed(_ => true) // It's required to use 'any origin' together with 'allow credentials'
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }
        private static void ConfigureWebApi(this IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    //options.Filters.Add(typeof(ValidateModelStateAttribute));
                })
                .AddFluentValidation(fv =>
                {
                    fv.DisableDataAnnotationsValidation = true;
                    fv.RegisterValidatorsFromAssembly(typeof(Startup).GetTypeInfo().Assembly);
                });

            // services.Configure<ApiBehaviorOptions>(options =>
            // {
            //     options.SuppressModelStateInvalidFilter = true;
            // });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Version = "v1.0.0",
                        Title = "AVStack.IdentityServer",
                        Description = "Simple IdentityServer built around IdentityServer4 and ASPNETIdentity",
                        Contact = new OpenApiContact()
                        {
                            Name = "Antonio Vukalović",
                            Email = "vukalovicantonio@gmail.com",
                        }
                    });
            });
        }
        private static void ConfigureMessageBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBus(options =>
            {
                options.Uri = new Uri(configuration.GetSection("RabbitMQ")["Uri"]);
            }, busFactory => busFactory.ConfigureTopology());
        }
        private static void ConfigureTopology(this IMessageBusFactory busFactory)
        {
            // Infrastructure
            busFactory.DeclareExchange("monitoring", ExchangeType.Topic);

            busFactory.DeclareExchange("identity-server", ExchangeType.Topic);
            busFactory.DeclareExchange("account", ExchangeType.Topic);

            busFactory.DeclareQueue("message-center");


            // Bindings
            busFactory.BindExchange("identity-server", "monitoring", "identity-server.#");
            busFactory.BindExchange("account", "monitoring", "account.#");

            busFactory.BindQueue("message-center", "identity-server", "#");
            busFactory.BindQueue("message-center", "account", "#");
        }
    }
}