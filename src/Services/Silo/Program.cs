using Microsoft.Extensions.Hosting;
using SiloProvider;

namespace SiloHost
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Configure the host
            using var app = Host.CreateDefaultBuilder(args).UseOrleansSilo().Build();
            
            await app.RunAsync();
        }
    }
}