using SiloProvider;

namespace MasterSilo;

internal class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseOrleansSilo();

        // Add services to the container.
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        var url = app.Configuration.GetValue<string>("Urls")!;

        app.UseHttpsRedirection();
        app.MapGet("/", () => $"Master Silo is up now on {url}");

        app.Run();
    }
}