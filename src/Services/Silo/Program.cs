using Microsoft.Extensions.Hosting;

namespace Silo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        //var builder = WebApplication.CreateBuilder(args);
        //builder.Host.UseOrleansSilo();

        // Configure the host
        await Host.CreateDefaultBuilder(args)
            .UseOrleansSilo()
            .Build()
            .RunAsync();
    }
}