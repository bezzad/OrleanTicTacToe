using GrainInterfaces.Extensions;
using Grains.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Net;
using System.Net.NetworkInformation;

namespace Silo;

public static class SiloHelper
{
    public static IHostBuilder UseOrleansSilo(this IHostBuilder host)
    {
        return host.UseOrleans(CreateSilo)
            .UseConsoleLifetime();
    }

    private static void CreateSilo(HostBuilderContext ctx, ISiloBuilder siloBuilder)
    {
        var config = ctx.GetOrleansConfig();

        siloBuilder
            .AddActivityPropagation()
            .UseAdoNetClustering(options =>
            {
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = config.AdoNetClusteringConnectionString;
            })
            .AddAdoNetGrainStorage("OrleansStorage", options =>
            {
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = config.AdoNetClusteringConnectionString;
            })
            .ConfigureLogging(logging => logging.AddConsole())
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = config.ClusterId;
                options.ServiceId = config.ServiceId;
            })
            .Configure<EndpointOptions>(options =>
            {
                // Port to use for silo-to-silo
                options.SiloPort = config.SiloPort;
                // Port to use for the gateway
                options.GatewayPort = config.GatewayPort;
                // IP Address to advertise in the cluster
                options.AdvertisedIPAddress = config.SiloAddress;
                // The socket used for client-to-silo will bind to this endpoint
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, config.GatewayPort);
                // The socket used by the gateway will bind to this endpoint
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, config.SiloPort);
            })
            //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences())
            .UseDashboard(options =>
            {
                options.HostSelf = false;
            })
            .AddGrainExtension<IGrainDeactivateExtension, GrainDeactivateExtension>();
            //.UsePerfCounterEnvironmentStatistics()
    }

    private static OrleansConfig GetOrleansConfig(this HostBuilderContext context)
    {
        var sqlConn = context.Configuration.GetValue<string>("OrleansConfig:OrleansDbConnectionString");
        var siloBasePort = context.Configuration.GetValue<int>("OrleansConfig:SiloPort");
        var instanceId = HostHelper.GetId(sqlConn, siloBasePort);
        var netType = context.Configuration.GetValue<NetworkInterfaceType>("OrleansConfig:NetworkInterfaceType");
        var siloAddress = HostHelper.GetAllLocalIPv4(netType).FirstOrDefault() ?? IPAddress.Loopback;

        return new OrleansConfig()
        {
            NetworkInterfaceType = netType,
            GatewayPort = context.Configuration.GetValue<int>("OrleansConfig:GatewayPort") + instanceId,
            SiloPort = siloBasePort + instanceId,
            ClusterId = context.Configuration.GetValue<string>("OrleansConfig:ClusterId"),
            ServiceId = context.Configuration.GetValue<string>("OrleansConfig:ServiceId"),
            AdoNetClusteringConnectionString = sqlConn,
            SiloAddress = siloAddress,
        };
    }
}
