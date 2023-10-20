using SiloProvider;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        var url = app.Configuration.GetValue<string>("Urls")!;

        app.UseHttpsRedirection();
        app.MapGet("/", () => $"Master Silo is up now on {url}");
        app.Map("/dashboard", x => x.UseOrleansDashboard());

        OpenBrowser(url + "/dashboard");

        await app.RunAsync(url);
    }

    public static void OpenBrowser(string url)
    {
        Task.Run(() =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true, CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        });
    }
}