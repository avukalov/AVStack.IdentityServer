using System;
using AVStack.IdentityServer.WebApi.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AVStack.IdentityServer.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Log.Logger = Helpers.ConfigureLogger();

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(Helpers.AddConfiguration())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
            //.UseSerilog();
        }
    }
}