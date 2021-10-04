using System;
using System.Reflection;
using AVStack.IdentityServer.Common.Models;
using AVStack.IdentityServer.Models.Interfaces;
using AVStack.IdentityServer.WebApi.Data;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Business;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;
using AVStack.IdentityServer.WebApi.Services;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using AVStack.MessageBus.Abstraction;
using AVStack.MessageBus.Extensions;
using FluentValidation.AspNetCore;
using IdentityServer4.Configuration;
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
            services.ConfigureCors();
            services.ConfigureNpgsql(configuration);
            services.ConfigureIdentity(configuration);
            services.ConfigureIdentityServer(configuration);
            
            services.ConfigureWebApi();
            services.ConfigureMessageBus(configuration);

            services.AddAutoMapper(typeof(Startup));
            
            services.RegisterModels();
            services.RegisterServices();
        }

        private static void RegisterModels(this IServiceCollection services)
        {
            services.AddScoped<IUser, User>();
            services.AddScoped<IIdentityMessage, IdentityMessage>();
        }
        
        private static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
        }

        private static void ConfigureNpgsql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AccountDbContext>(options =>
                options
                    .UseNpgsql(
                        configuration.GetSection("ConnectionStrings")["AVAccount"],
                        option =>
                        {
                            option.MigrationsAssembly(typeof(AccountDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        }));
                    //.ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore)));

        }

        private static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: !!!IMPORTANT!!!  Bind options from appsettings.json to objects and serve them as singletons
            services
                .AddIdentity<UserEntity, RoleEntity>(option =>
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
                        DeviceVerificationUrl = configuration.GetSection("IdentityServerOptions")["UserInteraction:DeviceVerificationUrl"],
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

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                })
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential();
        }

        private static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyCorsPolicy", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
        }

        private static void ConfigureWebApi(this IServiceCollection services)
        {
            // services.AddControllers();
            services.AddControllersWithViews().AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssembly(typeof(Startup).GetTypeInfo().Assembly);
                fv.DisableDataAnnotationsValidation = true;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", 
                    new OpenApiInfo
                    {
                        Title = "AVStack.IdentityServer.WebApi", 
                        Version = "v1"
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
            busFactory.DeclareExchange("email.sms.viber", ExchangeType.Topic);
            busFactory.DeclareQueue("email");
            busFactory.DeclareQueue("sms");
            busFactory.DeclareQueue("viber");
            busFactory.BindQueue("email", "email.sms.viber", "email.*.*");
            busFactory.BindQueue("sms", "email.sms.viber", "*.sms.*");
            busFactory.BindQueue("viber", "email.sms.viber", "*.*.viber");
        }
    }
}