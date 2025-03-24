using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace GeoHashDaemon
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    //config.AddInMemoryCollection(arrayDict);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile("Secrets\\secrets.json", optional: false, reloadOnChange: true);
                    config.AddCommandLine(args); // Not used now, but cool.
                })
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<GeoHashNotifier>();

                    if (WindowsServiceHelpers.IsWindowsService())
                        services.AddSingleton<IHostLifetime, MyServiceLifetime>();
                });
    }
}

