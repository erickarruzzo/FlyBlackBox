using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Fly.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var contentRoot = Directory.GetCurrentDirectory();
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseWebRoot(webRoot)
                .UseContentRoot(contentRoot)
                .UseWebListener()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
