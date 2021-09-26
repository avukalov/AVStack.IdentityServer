using System.Linq;
using AVStack.IdentityServer.Constants;
using AVStack.IdentityServer.WebApi.Data;
using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AVStack.IdentityServer.WebApi.Extensions
{
    public static class ApplicationExtensions
    {
        public static void InitialMaintenance(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope())
            {
                if (serviceScope != null)
                {
                    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();
                    
                    if (!context.Roles.Any())
                    {
                        context.Roles.Add(new RoleEntity
                        {
                            Name = IdentityRoleDefaults.Administrator,
                            NormalizedName = IdentityRoleDefaults.Administrator.ToUpper(),
                            Level = RoleLevel.Administrator
                            
                        });
                        context.Roles.Add(new RoleEntity
                        {
                            Name = IdentityRoleDefaults.User,
                            NormalizedName = IdentityRoleDefaults.User.ToUpper(),
                            Level = RoleLevel.User
                        });
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}