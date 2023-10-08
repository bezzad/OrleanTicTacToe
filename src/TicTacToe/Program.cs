using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using Orleans.Configuration;
using System.Net;

namespace TicTacToe;

public class Program
{
    public static async Task Main(string[] args) =>
            await CreateHostBuilder(args).Build().RunAsync().ConfigureAwait(false);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseOrleans((ctx, siloBuilder) =>
            {
                // In order to support multiple hosts forming a cluster,
                // they must listen on different ports.
                // Use the --InstanceId X option to launch subsequent hosts.
                var instanceId = ctx.Configuration.GetValue<int>("InstanceId");
                var port = 11_111;
                siloBuilder.UseLocalhostClustering(
                    // Port to use for silo-to-silo
                    siloPort: port + instanceId,
                    // Port to use for the gateway (Client-to-silo)
                    gatewayPort: 30000 + instanceId,
                    // The socket used for client-to-silo will bind to this endpoint
                    primarySiloEndpoint: new IPEndPoint(IPAddress.Loopback, port),
                    serviceId: "dev" + instanceId,
                    clusterId: "tictactoe_" + instanceId)
                   .ConfigureLogging(logging => logging.AddConsole());
                //siloBuilder.Configure<ClusterOptions>(options =>
                //{
                //    options.ClusterId = "my-first-cluster";
                //    options.ServiceId = "SampleApp";
                //});
                //siloBuilder.Configure<EndpointOptions>(options =>
                //{
                //    // Port to use for silo-to-silo
                //    options.SiloPort = 11_111;
                //    // Port to use for the gateway
                //    options.GatewayPort = 30_000;
                //    // IP Address to advertise in the cluster
                //    options.AdvertisedIPAddress = IPAddress.Parse("172.16.0.42");
                //    // The socket used for client-to-silo will bind to this endpoint
                //    options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 40_000);
                //    // The socket used by the gateway will bind to this endpoint
                //    options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 50_000);
                //});
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
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    // Add services to the container.
                    services.AddControllers();
                    services.AddSignalR();

                    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                    services.AddEndpointsApiExplorer();
                    services.AddSwaggerGen(options =>
                    {
                        var version = "v" + typeof(Program).Assembly.GetName().Version.ToString(3);
                        options.SwaggerDoc("v1", new OpenApiInfo
                        {
                            Title = nameof(TicTacToe),
                            Version = version
                        });
                        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"));
                        options.DescribeAllParametersInCamelCase();
                    });
                });

                webBuilder.UseStartup<Startup>();

                webBuilder.ConfigureKestrel((ctx, kestrelOptions) =>
                {
                    // To avoid port conflicts, each Web server must listen on a different port.
                    var instanceId = ctx.Configuration.GetValue<int>("InstanceId");
                    kestrelOptions.Listen(IPAddress.Any, 5000 + instanceId, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                    });
                });
            })
            .ConfigureServices((context, services) =>
            {
                //services.Configure<SampleOptions>(options => context.Configuration.GetSection("Sample").Bind(options));
            });

        return builder;
    }
}