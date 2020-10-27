using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace e.stock.api
{
    public class Program
    {
        public const string AllowSpecificOrigins = "_AllowSpecificOrigins";
        public static Uri AthenticateApi { get { return new Uri(@"https://authentic-api.herokuapp.com/"); } }

        public static int DataBasePort { get; internal set; }
        public static string DataBaseUser { get; internal set; }
        public static string DataBaseHost { get; internal set; }
        public static string DataBasePws { get; internal set; }
        public static string DataBaseAuth { get; internal set; }
        public static string DataBaseName { get; internal set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
