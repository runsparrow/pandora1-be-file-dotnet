using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace pandora1_be_file_dotnet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Threading.ThreadPool.SetMinThreads(500, 500);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseKestrel(options =>
                     {
                         options.Listen(IPAddress.Any, 8003, listenOptions =>
                         {
                             listenOptions.UseHttps("t-pic-cn-iis-1128142825.pfx", "t-pic.cn");
                         });
                     })
                    .UseStartup<Startup>();
                });
    }
}
