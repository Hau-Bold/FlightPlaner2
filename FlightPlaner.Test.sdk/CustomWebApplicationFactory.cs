using Microsoft.AspNetCore.Mvc.Testing;

namespace FlightPlaner.Test.sdk;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
        });
    }
}
