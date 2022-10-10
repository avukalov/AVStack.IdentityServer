using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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
using IdentityServer4.Test;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using AVStack.IdentityServer.WebApi.Controllers;
using AVStack.IdentityServer.WebApi.Models.Options;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

            services.RegisterOptions(configuration);
            services.RegisterModels();
            services.RegisterServices();
        }
        
        
        private static void RegisterOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AccountOptions>(configuration.GetSection(nameof(AccountOptions)));
            services.Configure<IdentityOptions>(configuration.GetSection("AspNetIdentityOptions"));
        }
        
        private static void RegisterModels(this IServiceCollection services)
        {
            services.AddScoped<IUser, User>();
        }
        private static void RegisterServices(this IServiceCollection services)
        {
            // services.AddTransient<IReturnUrlParser, ReturnUrlParser>();
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
                    
                    option.User = new UserOptions()
                    {
                        RequireUniqueEmail = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["UserOptions:RequireUniqueEmail"]),
                        AllowedUserNameCharacters = configuration.GetSection("AspNetIdentityOptions")["UserOptions:AllowedUserNameCharacters"]
                    };
                    option.SignIn = new SignInOptions()
                    {
                        RequireConfirmedAccount = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["SignInOptions:RequireConfirmedAccount"]),
                        RequireConfirmedEmail = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["SignInOptions:RequireConfirmedEmail"]),
                        RequireConfirmedPhoneNumber = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["SignInOptions:RequireConfirmedPhoneNumber"]),
                    };
                    option.Password = new PasswordOptions
                    {
                        RequiredLength = int.Parse(configuration.GetSection("AspNetIdentityOptions")["PasswordOptions:RequiredLength"]),
                        RequireDigit = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["PasswordOptions:RequireDigit"]),
                        RequiredUniqueChars = int.Parse(configuration.GetSection("AspNetIdentityOptions")["PasswordOptions:RequiredUniqueChars"]),
						RequireNonAlphanumeric = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["PasswordOptions:RequireNonAlphanumeric"]),
                        RequireUppercase = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["PasswordOptions:RequireUppercase"]),
                        RequireLowercase = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["PasswordOptions:RequireLowercase"])
                    };
                    option.Lockout = new LockoutOptions
                    {
                        AllowedForNewUsers = Convert.ToBoolean(configuration.GetSection("AspNetIdentityOptions")["LockoutOptions:AllowedForNewUsers"]),
                        MaxFailedAccessAttempts = int.Parse(configuration.GetSection("AspNetIdentityOptions")["LockoutOptions:MaxFailedAccessAttempts"]),
                        DefaultLockoutTimeSpan = TimeSpan.FromDays(365),
                    };
                    
                })
                .AddEntityFrameworkStores<AccountDbContext>()
                .AddDefaultTokenProviders();
        }
        private static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
			string thumbprint = configuration.GetSection("Certificates")["Thumbprint"];
            X509Certificate2 certificate2 = null;
            using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                certStore.Open(OpenFlags.ReadOnly);
                var certCollection = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    thumbprint,
                    false);
                if (certCollection.Count > 0)
                {
                    certificate2 = certCollection[0];
                }
            }

            var key = certificate2.PrivateKey;
            services
                .AddIdentityServer(options =>
                {
                    options.Authentication = new AuthenticationOptions()
                    {
                        CookieLifetime = TimeSpan.FromDays(365),
                        // CookieSlidingExpiration = true,
                    };
                    options.Events = new EventsOptions
                    {
                        RaiseErrorEvents = true,
                        RaiseFailureEvents = true,
                        RaiseSuccessEvents = false,
                        RaiseInformationEvents = false
                    };
                })
                // TODO: Add self sign cert for docker
                .AddSigningCredential(certificate2)
                //.AddDeveloperSigningCredential()
                
                .AddAspNetIdentity<UserEntity>()
                // Clients, Apis ...
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(configuration.GetSection("ConnectionStrings")["AVIdentityServer"], option =>
                        {
                            option.MigrationsAssembly(typeof(AccountDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        });
                })
                // Codes, Tokens, Consents ...
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(configuration.GetSection("ConnectionStrings")["AVIdentityServer"], option =>
                        {
                            option.MigrationsAssembly(typeof(AccountDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        });

                    // This enables automatic token cleanup. This is optional.
                    // options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 30;
                })
                .AddProfileService<ProfileService>()
                // TODO: Add custom ProfileService 
                ;

        }
        private static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Development", policy =>
                {
                    policy
                        // .WithOrigins("http://localhost:4200", "http://localhost:4300")
                        // .SetIsOriginAllowed(_ => true) // It's required to use 'any origin' together with 'allow credentials'
                        // .AllowCredentials();
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // services.AddSingleton<ICorsPolicyService>((container) => {
            //     var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
            //     return new DefaultCorsPolicyService(logger) {
            //         AllowAll = true,
            //         //AllowedOrigins = { "http://localhost:4200", "http://localhost:4201" }
            //     };
            // });
        }
        private static void ConfigureWebApi(this IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
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

            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1",
            //         new OpenApiInfo
            //         {
            //             Version = "v1.0.0",
            //             Title = "AVStack.IdentityServer",
            //             Description = "Simple IdentityServer built around IdentityServer4 and ASPNETIdentity",
            //             Contact = new OpenApiContact()
            //             {
            //                 Name = "Antonio Vukalović",
            //                 Email = "vukalovicantonio@gmail.com",
            //             }
            //         });
            // });
        }
        private static void ConfigureMessageBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBus(options =>
            {
                options.Uri = new Uri(configuration.GetSection("RabbitMQ")["DevUri"]);
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