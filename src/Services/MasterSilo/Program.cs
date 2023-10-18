using SiloProvider;

namespace MasterSilo;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseOrleansSilo();

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.Map("/dashboard", x => x.UseOrleansDashboard());
        app.UseOrleansDashboard();

        app.Run(x =>
        {
            x.Response.Redirect("http://127.0.0.1:8080/dashboard");
            return Task.CompletedTask;
        });

        await app.RunAsync();
    }
}