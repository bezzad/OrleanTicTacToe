using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Silo;

internal class Program
{
    static void Main(string[] args)
    {
        //var builder = WebApplication.CreateBuilder(args);
        //builder.Host.UseOrleansSilo();

        // Configure the host
        using var app = Host.CreateDefaultBuilder(args).UseOrleansSilo().Build();

        app.Run();
    }
}