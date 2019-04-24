using System;
using System.IO;
using System.Threading.Tasks;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using ServiceStack;
using Trov.GildedRose.Api.Core;
using Trov.GildedRose.Api.Services;

namespace Trov.GildedRose.Api.Host
{
    public class Program
    {
        public static void Main()
        {
            var hostConfiguration = BuildConfiguration(new ConfigurationBuilder()).Build();

            var hostBuilder = new WebHostBuilder().UseKestrel()
                                                  .UseContentRoot(Directory.GetCurrentDirectory())
                                                  .UseStartup<GildedHostStartup>()
                                                  .UseConfiguration(hostConfiguration)
                                                  .UseShutdownTimeout(TimeSpan.FromSeconds(15))
                                                  .UseUrls("http://*:8888")
                                                  .ConfigureAppConfiguration((wc, conf) => BuildConfiguration(conf))
                                                  .ConfigureLogging(b => b.ClearProviders()
                                                                          .AddNLog()
                                                                          .SetMinimumLevel(LogLevel.Debug));

            var host = hostBuilder.Build();

            host.Run();
        }

        private static IConfigurationBuilder BuildConfiguration(IConfigurationBuilder conf)
            => conf.AddJsonFile("appsettings.json", false, true)
                   .AddJsonFile($"appsettings.{TrovEnvironment.Configuration}.json", true, true);
    }

    public class GildedHostStartup
    {
        private ApiAppHost _appHost;

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var appLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            TrovEnvironment.Init(new NetCoreAppSettings(configuration));

            _appHost = new ApiAppHost
                       {
                           AppSettings = TrovEnvironment.AppSettings
                       };

            app.UseServiceStack(_appHost);

            appLifetime.ApplicationStopping.Register(OnShutdown);

            app.Run(context =>
                    {
                        context.Response.Redirect("/metadata");

                        return Task.FromResult(0);
                    });

            TrovAppHostConfig.Instance.Log.Info("Listening for requests...");
        }

        private void OnShutdown() => _appHost.Shutdown();
    }

    public class ApiAppHost : AppHostBase
    {
        public ApiAppHost() : base("Trov GildedRose Self Hosted API", typeof(ApiAppHost).Assembly) { }

        public override void Configure(Container container)
        {
            TrovAppHostConfig.Instance.Configure(this, container);
        }

        public void Shutdown() => TrovAppHostConfig.Instance.Shutdown(Container);
    }
}
