using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Net;

namespace SiloProvider;

public static class SiloHelper
{
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

        siloBuilder.AddActivityPropagation();

        return siloBuilder
        // .UseLocalhostClustering()
         .UseAdoNetClustering(options =>
         {
             options.Invariant = "System.Data.SqlClient";
             options.ConnectionString = ctx.Configuration.GetConnectionString("OrleansDb");
         })
        //.UseRedisClustering(ctx.Configuration.GetConnectionString("Redis"))
        .ConfigureLogging(logging => logging.AddConsole())
        //.UseLinuxEnvironmentStatistics()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "orleansClusterTictactoe";
            options.ServiceId = "orleansServiceTictactoe";
        })
        .Configure<EndpointOptions>(options =>
        {
            // Port to use for silo-to-silo
            options.SiloPort = 11_111 + instanceId;
            // Port to use for the gateway
            options.GatewayPort = 30_000 + instanceId;
            // IP Address to advertise in the cluster
            options.AdvertisedIPAddress = IPAddress.Loopback;
            // The socket used for client-to-silo will bind to this endpoint
            //options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 40_000 + instanceId);
            // The socket used by the gateway will bind to this endpoint
            //options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 50_000 + instanceId);
        })
        .UseDashboard(options =>
        {
            // https://github.com/OrleansContrib/OrleansDashboard
            //options.Username = "bezzad";
            //options.Password = "1234";
            options.Host = "*";
            options.Port = 8080 + instanceId;
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 1000;
        });
        //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences());
    }

    private static int GetInstanceId(this string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Trim(' ', '-').Equals("instanceid", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(args[i + 1], out var instanceId))
            {
                return instanceId;
            }
        }

        return 0;
    }
}
