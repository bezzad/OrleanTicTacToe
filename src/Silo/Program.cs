using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Net;

// Configure the host
using IHost host = Host.CreateDefaultBuilder(args)
    .UseOrleans((ctx, siloBuilder) =>
    {
        // In order to support multiple hosts forming a cluster,
        // they must listen on different ports.
        // Use the --InstanceId X option to launch subsequent hosts.

        var instanceId = ctx.Configuration.GetValue<int>("InstanceId");
        siloBuilder.UseLocalhostClustering().ConfigureLogging(logging => logging.AddConsole());
        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "tictactoe";
            options.ServiceId = "dev";
        });
        siloBuilder.Configure<EndpointOptions>(options =>
        {
            // Port to use for silo-to-silo
            options.SiloPort = 11_111;
            // Port to use for the gateway
            options.GatewayPort = 30_000;
            // IP Address to advertise in the cluster
            // options.AdvertisedIPAddress = IPAddress.Parse("172.16.0.42");
            // The socket used for client-to-silo will bind to this endpoint
            options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 30_000);
            // The socket used by the gateway will bind to this endpoint
            options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 50_000);
        });
        siloBuilder.UseDashboard(options =>
        {
            // https://github.com/OrleansContrib/OrleansDashboard
            options.Username = "bezzad";
            options.Password = "1234";
            options.Host = "*";
            options.Port = 8080;
            options.HostSelf = true;
            options.CounterUpdateIntervalMs = 1000;
        });
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();