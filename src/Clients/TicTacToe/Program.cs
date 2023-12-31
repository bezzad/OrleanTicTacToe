using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using Orleans.Configuration;
using System.Net;

namespace TicTacToe;

internal class Program
{
    static async Task Main(string[] args) =>
            await CreateHostBuilder(args).Build().RunAsync().ConfigureAwait(false);

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseOrleansClient((ctx, clientBuilder) =>
            {
                clientBuilder
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = ctx.Configuration.GetConnectionString("OrleansDb");
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "orleansClusterTictactoe";
                    options.ServiceId = "orleansServiceTictactoe";
                });
            })
            .ConfigureLogging(logging => logging.AddConsole())
            .ConfigureWebHostDefaults(webBuilder =>
            {
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
                // Add services to the container.
                services.AddControllers();
                services.AddServicesForSelfHostedDashboard();
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

        return builder;
    }
}