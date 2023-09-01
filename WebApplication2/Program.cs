using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using AdvertisingAgency.Data.Data;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace AdvertisingAgency.Web
{
    /// <summary>
    /// Represents the entry point of the application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates an instance of the host builder with default configurations.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>An instance of the host builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}