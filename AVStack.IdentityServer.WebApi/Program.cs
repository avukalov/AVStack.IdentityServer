using System;
using AVStack.IdentityServer.WebApi.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AVStack.IdentityServer.WebApi
{
    public class Program
    {
        private static int ExitCode { get; set; }
        public static int Main(string[] args)
        {
            Log.Logger = Helpers.ConfigureLogger();

            try
            {
                Log.Information("Starting host");
                CreateHost(args).Run();
                ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                ExitCode = 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return ExitCode;
        }

        private static IHost CreateHost(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(Helpers.AddConfiguration())
                .ConfigureWebHostDefaults(wb => wb.UseStartup<Startup>())
                .UseSerilog()
                .Build();
        }
    }
}