
using System.Net;

namespace TicTacToe;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        await builder.UseOrleans((ctx, siloBuilder) =>
            {
                // In order to support multiple hosts forming a cluster,
                // they must listen on different ports.
                // Use the --InstanceId X option to launch subsequent hosts.
                var instanceId = ctx.Configuration.GetValue<int>("InstanceId");
                var port = 11_111;
                siloBuilder.UseLocalhostClustering(
                    siloPort: port + instanceId,
                    gatewayPort: 30000 + instanceId,
                    primarySiloEndpoint: new IPEndPoint(IPAddress.Loopback, port),
                    serviceId: "dev" + instanceId,
                    clusterId: "tictactoe_" + instanceId)
                   .ConfigureLogging(logging => logging.AddConsole());
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

                    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                    services.AddEndpointsApiExplorer();
                    services.AddSwaggerGen();
                });

                webBuilder.UseStartup<Startup>();

                webBuilder.ConfigureKestrel((ctx, kestrelOptions) =>
                {
                    // To avoid port conflicts, each Web server must listen on a different port.
                    var instanceId = ctx.Configuration.GetValue<int>("InstanceId");
                    //kestrelOptions.ListenLocalhost(5001 + instanceId);
                    kestrelOptions.ListenAnyIP(5001 + instanceId);
                });
            })
            .RunConsoleAsync();
    }
}