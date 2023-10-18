using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using System.Net;

namespace TicTacToe;

public class Program
{
    public static async Task Main(string[] args) =>
            await CreateHostBuilder(args).Build().RunAsync().ConfigureAwait(false);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseOrleansClient(client =>
            {
                client.UseLocalhostClustering(30_000, serviceId: "orleansServiceTictactoe", clusterId: "orleansClusterTictactoe");
            })
            .ConfigureLogging(logging => logging.AddConsole())
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
                //services.Configure<IClusterClient>(options => context.Configuration.GetSection("Sample").Bind(options));
            });

        return builder;
    }
}