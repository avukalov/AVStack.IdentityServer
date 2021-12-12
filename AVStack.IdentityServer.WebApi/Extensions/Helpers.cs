using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AVStack.IdentityServer.WebApi.Extensions
{
    public static class Helpers
    {
        public static IConfigurationRoot BuildConfiguration(string settingsFileName, string environment = "Development")
        {
            return new ConfigurationBuilder()
                .AddJsonFile($"{settingsFileName}.json", false, true)
                .AddJsonFile($"{settingsFileName}.{environment}.json", true, true)
                .Build();
        }

        public static Logger ConfigureLogger()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = BuildConfiguration("appsettings", environment);

            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public static Action<IConfigurationBuilder> AddConfiguration(string settingsFileName = "appsettings")
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return configuration =>
            {
                configuration.AddJsonFile($"{settingsFileName}.json", false, true);
                configuration.AddJsonFile($"{settingsFileName}.{environment ?? "Development"}.json", true, true);
            };
        }

        public static bool IsEmail(this string email)
        {
            if (email.Trim().EndsWith(".")) {
                return false; // suggested by @TK-421
            }
            try {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch {
                return false;
            }
        }


    }
}