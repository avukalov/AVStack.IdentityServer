using System;
using System.IO;
using AVStack.IdentityServer.WebApi.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
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
                .ConfigureWebHostDefaults(wb =>
                {
                    wb.UseKestrel();
                    wb.UseContentRoot(Directory.GetCurrentDirectory());
                    wb.UseUrls(
                        // HTTP
                        "http://localhost:5004",
                        "http://192.168.1.2:5004",
                        // HTTPS
                        "https://localhost:5005",
                        "https://192.168.1.2:5005");
                    wb.UseStartup<Startup>();
                })
                //.UseSerilog()
                .Build();
        }
    }
}