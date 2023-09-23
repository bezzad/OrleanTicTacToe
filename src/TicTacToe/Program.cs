using Microsoft.OpenApi.Models;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

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
                    kestrelOptions.ListenAnyIP(5000 + instanceId);
                });
            })
            .ConfigureServices((context, services) =>
            {
                //services.Configure<SampleOptions>(options => context.Configuration.GetSection("Sample").Bind(options));
            });

        return builder;
    }
}