using AVStack.IdentityServer.WebApi.Data;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Constants;
using AVStack.IdentityServer.WebApi.Models.Enums;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Classes
using Client = IdentityServer4.Models.Client;
using ApiResource = IdentityServer4.Models.ApiResource;
using ApiScope = IdentityServer4.Models.ApiScope;
using Secret = IdentityServer4.Models.Secret;
using ClientEntity = IdentityServer4.EntityFramework.Entities.Client;
using ApiResourceEntity = IdentityServer4.EntityFramework.Entities.ApiResource;
using ApiScopeEntity = IdentityServer4.EntityFramework.Entities.ApiScope;
using IdentityResourceEntity = IdentityServer4.EntityFramework.Entities.IdentityResource;

namespace AVStack.IdentityServer.WebApi.Extensions
{
    public static class ApplicationExtensions
    {
        public static void ConfigureApplication(this IApplicationBuilder app)
        {
            app.UseCors("Development");
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseStaticFiles();

            app.UseIdentityServer();
            //app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
        public static void ConfigureDevelopmentApplication(this IApplicationBuilder app)
        {
            //app.ApplyMigrations();
            app.InitialSeed();

            app.UseDeveloperExceptionPage();
            // app.UseSwagger();
            // app.UseSwaggerUI(c =>
            //     c.SwaggerEndpoint(
            //         "/swagger/v1/swagger.json",
            //         "AVStack.IdentityServer.WebApi v1"
            //     ));
        }
        private static void ApplyMigrations(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                if (serviceScope != null)
                {
                    var identityContext = serviceScope.ServiceProvider.GetRequiredService<AccountDbContext>();
                    var configurationContext = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                    var persistedGrantContext = serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();

                    identityContext.Database.EnsureCreated();
                    configurationContext.Database.EnsureCreated();
                    persistedGrantContext.Database.EnsureCreated();
                    
                    // TODO: Apply Migration only if there is no tables present
                    identityContext.Database.Migrate();
                    configurationContext.Database.Migrate();
                    persistedGrantContext.Database.Migrate();
                }
            }
        }
        private static void InitialSeed(this IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                if (serviceScope != null)
                {
                    SeedRoles(serviceScope);
                    SeedUsers(serviceScope);
                    SeedApiResources(serviceScope);
                    SeedApiScopes(serviceScope);
                    SeedIdentityResources(serviceScope);
                    SeedClients(serviceScope);
                }
            }
        }
        private static void SeedApiResources(IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            if (!context.ApiResources.Any())
            {
                var apiResources = new List<ApiResourceEntity>
                {
                    // Identity Server
                    new ApiResource("avstack.identity-server.api", "AVStack IdentityServer WebApi").ToEntity(),
                    // new ApiResource("avstack.identity-server.api.read", "AVStack IdentityServer Read").ToEntity(),
                    // new ApiResource("avstack.identity-server.api.write", "AVStack IdentityServer Write").ToEntity(),
                    // new ApiResource("avstack.identity-server.api.full", "AVStack IdentityServer Full").ToEntity(),

                    // Message Center
                    new ApiResource("avstack.message-center.api", "AVStack MessageCenter WebApi").ToEntity(),
                    new ApiResource("avcloud.api", "AVCloud WebApi")
                    {
                        Scopes =
                        {
                            "avcloud.api.create",
                            "avcloud.api.read",
                            "avcloud.api.update",
                            "avcloud.api.delete",
                            "avcloud.api.full"
                        }
                    }.ToEntity(),
                };
                context.ApiResources.AddRange(apiResources);
                context.SaveChanges();
            }
        }
        private static void SeedIdentityResources(IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            if (!context.IdentityResources.Any())
            {
                var identityResources = new List<IdentityResourceEntity>
                {
                    new IdentityResources.OpenId().ToEntity(),
                    new IdentityResources.Profile().ToEntity(),
                    new IdentityResources.Address().ToEntity(),
                    new IdentityResources.Email().ToEntity(),
                    new IdentityResources.Phone().ToEntity(),
                };
                context.IdentityResources.AddRange(identityResources);
                context.SaveChanges();
            }
        }

        private static void SeedApiScopes(IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            if (!context.ApiScopes.Any())
            {
                var apiScopes = new List<ApiScopeEntity>
                {
                    new ApiScope()
                    {
                        Name = "avcloud.api.create",
                        DisplayName = "AVCloud Create Scope",
                    }.ToEntity(),
                    new ApiScope()
                    {
                        Name = "avcloud.api.read",
                        DisplayName = "AVCloud Read Scope",
                    }.ToEntity(),
                    new ApiScope()
                    {
                        Name = "avcloud.api.update",
                        DisplayName = "AVCloud Update Scope",
                    }.ToEntity(),
                    new ApiScope()
                    {
                        Name = "avcloud.api.delete",
                        DisplayName = "AVCloud Delete Scope",
                    }.ToEntity(),
                    new ApiScope()
                    {
                        Name = "avcloud.api.full",
                        DisplayName = "AVCloud Full Scope",
                    }.ToEntity(),

                };
                context.ApiScopes.AddRange(apiScopes);
                context.SaveChanges();
            }
        }

        private static void SeedClients(IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            if (!context.Clients.Any())
            {
                var clients = new List<ClientEntity>
                {
                    // AVStack Account Service
                    new Client
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "avstack.accounts.api",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets = new List<Secret>
                        {
                            new Secret("avstack.accounts.api".Sha512()),
                        },
                        AllowedScopes =
                        {
                            "avstack.message-center.api",
                        }
                    }.ToEntity(),

                    // AVStack.Angular SPA application
                    new Client
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "avstack.accounts.ui",
                        AllowedGrantTypes = GrantTypes.Code,
                        AllowOfflineAccess = true,
                        RequireClientSecret = false,
                        RequirePkce = true,
                        AllowAccessTokensViaBrowser = true,
                        RequireConsent = false,
                        AccessTokenLifetime = 600,
                        RedirectUris = new List<string>
                        {
                            "http://localhost:4200/account/signin-callback",
                            "http://localhost:4200/assets/silent-callback.html"
                        },
                        PostLogoutRedirectUris = new List<string>
                        {
                            "http://localhost:4200/account/signout-callback"
                        },
                        AllowedCorsOrigins = { "http://localhost:4200" },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Address,
                            IdentityServerConstants.StandardScopes.Email,
                            IdentityServerConstants.StandardScopes.Phone,
                            IdentityServerConstants.StandardScopes.OfflineAccess
                        },

                    }.ToEntity(),
                    
                    // Angular SPA testing application
                    new Client
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = "avstack.test.ui",
                        AllowedGrantTypes = GrantTypes.Code,
                        RequireClientSecret = false,
                        RequirePkce = true,
                        AllowAccessTokensViaBrowser = true,
                        RequireConsent = false,
                        AccessTokenLifetime = 6000,
                        FrontChannelLogoutUri = "http://localhost:4300/front-channel-signout-callback",
                        RedirectUris = new List<string>
                        {
                            "http://localhost:4300/signin-callback",
                            "http://localhost:4300/assets/silent-callback.html"
                        },
                        PostLogoutRedirectUris = new List<string>
                        {
                            "http://localhost:4300/signout-callback"
                        },
                        AllowedCorsOrigins = { "http://localhost:4200", "http://localhost:4300" },
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.Email,
                        },
                    }.ToEntity(),

                };
                context.Clients.AddRange(clients);
                context.SaveChanges();
            }
        }
        private static void SeedUsers(IServiceScope serviceScope)
        {
            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
            if (!userManager.Users.Any())
            {
                var superAdministrator = new UserEntity()
                {
                    FirstName = "Super",
                    LastName = "Administrator",
                    Email = "superadmin@avstack.com",
                    UserName = "superadmin",
                    EmailConfirmed = true
                };
                
                if (Task.Run(() => userManager.CreateAsync(superAdministrator, "@V$tack.123!")).Result.Succeeded)
                    Task.Run(() => userManager.AddToRoleAsync(superAdministrator, IdentityRoleDefaults.SuperAdministrator)).Wait();
            }
        }
        private static void SeedRoles(IServiceScope serviceScope)
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<AccountDbContext>();
            if (!context.Roles.Any())
            {
                var roles = new List<RoleEntity>
                {
                    new()
                    {
                        Name = IdentityRoleDefaults.SuperAdministrator,
                        NormalizedName = IdentityRoleDefaults.SuperAdministrator.ToUpper(),
                        Level = RoleLevel.SuperAdministrator

                    },
                    new()
                    {
                        Name = IdentityRoleDefaults.Administrator,
                        NormalizedName = IdentityRoleDefaults.Administrator.ToUpper(),
                        Level = RoleLevel.Administrator

                    },
                    new()
                    {
                        Name = IdentityRoleDefaults.User,
                        NormalizedName = IdentityRoleDefaults.User.ToUpper(),
                        Level = RoleLevel.User
                    }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges();
            }
        }

    }
}