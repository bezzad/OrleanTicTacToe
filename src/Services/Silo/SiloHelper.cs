using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Net;

namespace Silo;

public static class SiloHelper
{
    const string ClusterId = "orleansClusterTictactoe";

    public static IHostBuilder UseOrleansSilo(this IHostBuilder host)
    {
        return host.UseOrleans((ctx, siloBuilder) => CreateSilo(ctx, siloBuilder))
            .UseConsoleLifetime();
    }

    private static ISiloBuilder CreateSilo(HostBuilderContext ctx, ISiloBuilder siloBuilder)
    {
        // In order to support multiple hosts forming a cluster,
        // they must listen on different ports.
        // Use the --InstanceId X option to launch subsequent hosts.
        var args = Environment.GetCommandLineArgs();
        var instanceId = args.GetInstanceId();
        var siloPort = 11_111 + instanceId;
        var gatewayPort = 30_000 + instanceId;
        var dashboardPort = 8080 + instanceId;

        siloBuilder.AddActivityPropagation()

        //.UseLocalhostClustering()
        //.UseRedisClustering(ctx.Configuration.GetConnectionString("Redis"))
        .UseAdoNetClustering(options =>
        {
            options.Invariant = "System.Data.SqlClient";
            options.ConnectionString = ctx.Configuration.GetConnectionString("OrleansDb");
        })
        .ConfigureLogging(logging => logging.AddConsole())
        //.UseLinuxEnvironmentStatistics()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = ClusterId;
            options.ServiceId = "orleansServiceTictactoe";
        })
        .Configure<EndpointOptions>(options =>
        {
            // Port to use for silo-to-silo
            options.SiloPort = siloPort;
            // Port to use for the gateway
            options.GatewayPort = gatewayPort;
            // IP Address to advertise in the cluster
            options.AdvertisedIPAddress = IPAddress.Loopback;
            // The socket used for client-to-silo will bind to this endpoint
            options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, gatewayPort);
            // The socket used by the gateway will bind to this endpoint
            options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, siloPort);
        })
        .UseDashboard(options =>
        {
            // https://github.com/OrleansContrib/OrleansDashboard
            //options.Username = "bezzad";
            //options.Password = "1234";
            options.Host = "*";
            options.Port = dashboardPort;
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 1000;
        });
        //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences());

        new Uri("http://127.0.0.1:" + dashboardPort).OpenBrowser();

        return siloBuilder;
    }
}
