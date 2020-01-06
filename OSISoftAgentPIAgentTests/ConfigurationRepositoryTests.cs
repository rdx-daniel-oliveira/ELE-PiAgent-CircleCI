using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Implementations;
using System;
using Xunit;

namespace OSISoftAgentPIAgentTests
{
    public class ConfigurationRepositoryTests
    {
        private ILogger<ConfigurationRepository> _log;
       
        public ConfigurationRepositoryTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            _log = factory.CreateLogger<ConfigurationRepository>();
        }

        /// <summary>
        /// Try to load configuration from file and test if all parameters were provided 
        /// </summary>
        [Fact]
        public void GetConfigurationFromFile()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json",EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_log);

            Assert.True(configurationRepository.GetConfiguration());
        }

        /// <summary>
        /// Try to load configuration from environment and test if all parameters were provided 
        /// </summary>
        [Fact]
        public void GetConfigurationFromEnvironment()
        {
            //actual values of these parameters won't matter here, only testing if everything is filled up ok.
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", @"", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("PI_DATA_ARCHIVE_NAME", "radixuspisandbox-centralpiserver.southcentralus.cloudapp.azure.com", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("PI_USER", "piser", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("PI_PASSWORD", "pipassword", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("E_USER", "euser", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("E_PASSWORD", "epassword", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("E_ORG_ID", "1", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("E_DATASET_ID", "1", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("E_AGENT_INTERVAL_MIN", "1", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("E_BASE_URL", "https://app.elementanalytics.com/api/", EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("PI_ATTRIBUTE_DEFINITION", "test", EnvironmentVariableTarget.User);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(_log);

            Assert.True(configurationRepository.GetConfiguration());
        }

    }
}
