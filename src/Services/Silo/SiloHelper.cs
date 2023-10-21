using GrainInterfaces.Extensions;
using Grains.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Net;

namespace Silo;

public static class SiloHelper
{
    private const string ClusterId = "orleansClusterTictactoe"; // (ClusterId + ServiceId) are used for cluster membership
    private const string ServiceId = "orleansTictactoeService"; // ServiceId is used for storage
    private static readonly int GatewayStartPort = 30_000;
    private static readonly int SiloStartPort = 11_111;
    private static readonly IPAddress SiloAddress = HostHelper.GetLocalIPAddress(); // IPAddress.Parse("192.168.1.104"); // IPAddress.Loopback;

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
        var sqlConn = ctx.Configuration.GetConnectionString("OrleansDb");
        var instanceId = HostHelper.GetId(sqlConn, SiloStartPort);
        var siloPort = SiloStartPort + instanceId;
        var gatewayPort = GatewayStartPort + instanceId;

        siloBuilder.AddActivityPropagation()
        //.UseLocalhostClustering()
        //.UseRedisClustering(ctx.Configuration.GetConnectionString("Redis"))
        .UseAdoNetClustering(options =>
        {
            options.Invariant = "System.Data.SqlClient";
            options.ConnectionString = sqlConn;
        })
        .AddAdoNetGrainStorage("OrleansStorage", options =>
        {
            options.Invariant = "System.Data.SqlClient";
            options.ConnectionString = sqlConn;
        })
        .ConfigureLogging(logging => logging.AddConsole())
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = ClusterId;
            options.ServiceId = ServiceId;
        })
        .Configure<EndpointOptions>(options =>
        {
            // Port to use for silo-to-silo
            options.SiloPort = siloPort;
            // Port to use for the gateway
            options.GatewayPort = gatewayPort;
            // IP Address to advertise in the cluster
            options.AdvertisedIPAddress = SiloAddress;
            // The socket used for client-to-silo will bind to this endpoint
            options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, gatewayPort);
            // The socket used by the gateway will bind to this endpoint
            options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, siloPort);
        })
        //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences())
        .UseDashboard(options =>
        {
            options.HostSelf = false;
        })
        .AddGrainExtension<IGrainDeactivateExtension, GrainDeactivateExtension>();
        //.UsePerfCounterEnvironmentStatistics()
        return siloBuilder;
    }
}
