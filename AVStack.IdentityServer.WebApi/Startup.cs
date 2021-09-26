using System.IdentityModel.Tokens.Jwt;
using AVStack.IdentityServer.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AVStack.IdentityServer.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
            services.ConfigureCors();
            services.ConfigureNpgsql(Configuration);
            services.ConfigureIdentity();
            services.ConfigureMessageBus(Configuration);
            
            services.ConfigureIdentityServer(options =>
            {
                options.IdentityResources = InMemoryConfig.GetIdentityResources();
                options.Clients = InMemoryConfig.GetClients();
            });
            
            services.ConfigureWebApi();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.InitialMaintenance();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint(
                        "/swagger/v1/swagger.json", 
                        "AVStack.IdentityServer.WebApi v1"
                    ));
            }
            
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}