using System;
using System.Reflection;
using AVStack.IdentityServer.Configuration;
using AVStack.IdentityServer.WebApi.Data;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models;
using AVStack.IdentityServer.WebApi.Services;
using AVStack.MessageBus.Abstraction;
using AVStack.MessageBus.Extensions;
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
        public static void ConfigureNpgsql(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseNpgsql(
                        configuration.GetSection("ConnectionStrings")["IdentityServerDb"],
                        option =>
                        {
                            option.MigrationsAssembly(typeof(ApplicationDbContext).GetTypeInfo().Assembly.GetName().Name);
                            option.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                        }));
                    //.ConfigureWarnings(warnings => warnings.Default(WarningBehavior.Ignore)));
        }
        public static void ConfigureIdentity(this IServiceCollection services)
        {
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
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        }
        public static void ConfigureIdentityServer(this IServiceCollection services, Action<IdentityServerStore> optionsAction)
        {
            var identityServerStore = new IdentityServerStore();
            optionsAction.Invoke(identityServerStore);
            
            services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseFailureEvents = true;
                })
                .AddAspNetIdentity<UserEntity>()
                .AddInMemoryClients(identityServerStore.Clients)
                .AddInMemoryIdentityResources(identityServerStore.IdentityResources)
                // .AddInMemoryApiResources(identityServerStore.ApiResources)
                // .AddInMemoryApiScopes(identityServerStore.ApiScopes)
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential();
            
            // services.Configure<IdentityServerStore>(
            //     configuration.GetSection(IdentityServerStore.IdentityServerStoreSection));
        }
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }
        public static void ConfigureWebApi(this IServiceCollection services)
        {
            // services.AddControllers();
            services.AddControllersWithViews();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1", 
                    new OpenApiInfo
                    {
                        Title = "AVStack.IdentityServer.WebApi", 
                        Version = "v1"
                    });
            });
        }
        public static void ConfigureMessageBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessageBus(options =>
            {
                options.Uri = new Uri(configuration.GetSection("RabbitMQ")["Uri"]);
            }, busFactory =>
            {
                busFactory.ConfigureTopology();
            });
        }

        private static void ConfigureTopology(this IMessageBusFactory busFactory)
        {
            busFactory.DeclareExchange("confirmation.notification.newsletter", ExchangeType.Topic);
            busFactory.DeclareQueue("confirmation");
            busFactory.DeclareQueue("notification");
            busFactory.DeclareQueue("newsletter");
            busFactory.BindQueue("confirmation", "confirmation.notification.newsletter", "confirmation.*.*");
            busFactory.BindQueue("notification", "confirmation.notification.newsletter", "*.notification.*");
            busFactory.BindQueue("newsletter", "confirmation.notification.newsletter", "*.*.newsletter");
        }
    }
}