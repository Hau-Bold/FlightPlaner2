using FlightPlaner.Data;
using FlightPlaner.Services.Contract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace FlightPlaner.Test.sdk;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private IOptimizationService optimizationService;
    private IOpenStreetMapService openStreetMapService;

    public CustomWebApplicationFactory(IOptimizationService optimizationService,
                                       IOpenStreetMapService openStreetMapService)
    {
        this.optimizationService = optimizationService;
        this.openStreetMapService = openStreetMapService;
    }

    /// <summary>
    /// Workaround for ASP.Net core  bug https://github.com/dotnet/aspnetcore/issues/40271
    ///Host is stopped twice which causes iHostedservice.StopAsync to be called twice.
    /// </summary>
    internal  void  Stopapplication()
    {
        var hostLifetimeService = Services.GetRequiredService<IHostApplicationLifetime>();
        hostLifetimeService.StopApplication();
    }

    internal HttpClient StartApplication()
        => CreateClient();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        this.ConfigureAppSettings(builder);
        ClearDatabase(builder);


        return base.CreateHost(builder);
    }

    private void ClearDatabase(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var serviceprovider = services.BuildServiceProvider();
            using var scope = serviceprovider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<GPSDbContext>();
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IOpenStreetMapService));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddTransient(_ => optimizationService);
            services.AddTransient(_ => openStreetMapService);
        });
    }


    private void ConfigureAppSettings(IHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {

            });
        });
    }

    internal void Dispose()
    {
        base.Dispose();
    }
}
