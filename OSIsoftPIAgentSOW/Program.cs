using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Implementations;
using OSIsoftPIAgentSOW.Repositories.Interfaces;
using OSIsoftPIAgentSOW.Services.Implementations;
using OSIsoftPIAgentSOW.Services.Interfaces;
using OSIsoftPIAgentSOW.Utils.Implementations;
using OSIsoftPIAgentSOW.Utils.Interfaces;

using System;
using System.Threading;

namespace OSIsoftPIAgentSOW
{
    class Program
    {
        private static AutoResetEvent waitHandle = new AutoResetEvent(false);
        /// <summary>
        /// Entry point
        /// Configure dependency injections and call the service facade that will orchestrate the process
        /// </remarks>
        static void Main(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = configurationBuilder.Build();

            var collection = new ServiceCollection();
            collection.AddScoped<IHttpHelper, HttpHelper>();
            collection.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            collection.AddScoped<IAssetHubRepository, AssetHubRepository>();
            collection.AddScoped<IPIRepository, PIRepository>();
            collection.AddScoped<IFacadeService, FacadeService>();
            collection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
            });

                       
            using (var serviceProvider = collection.BuildServiceProvider())
            {
                 //Call the service facade that will orchestrate the process 
                Console.WriteLine("Agent started. Press control-c to end");

                IFacadeService service = serviceProvider.GetService<IFacadeService>();
                
                if (service.SetUp())
                {
                    //wait for input to exit. This is a way to run this application "as a service" in a container
                    Console.CancelKeyPress += (o, e) =>
                    {
                        waitHandle.Set();
                    };
                    waitHandle.WaitOne();
                }
                else
                {
                    // for CircleCI build test
                    Console.WriteLine("Hello world");
                }
            }
        }
    }
}
