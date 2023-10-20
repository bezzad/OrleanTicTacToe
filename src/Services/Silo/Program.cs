using Microsoft.Extensions.Hosting;
using SiloProvider;

namespace SiloHost;

internal class Program
{
    static void Main(string[] args)
    {
        // Configure the host
        using var app = Host.CreateDefaultBuilder(args).UseOrleansSilo().Build();

        app.Run();
    }
}