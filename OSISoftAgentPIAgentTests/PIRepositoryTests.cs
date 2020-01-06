using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OSISoftAgentPIAgentTests
{
    public class PIRepositoryTests
    {
        private ILogger<PIRepository> _logPI;
        private ILogger<ConfigurationRepository> _logConfiguration;

        public PIRepositoryTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            _logPI = factory.CreateLogger<PIRepository>();
            _logConfiguration = factory.CreateLogger<ConfigurationRepository>();
        }

        /// <summary>
        /// Try to connect with PI DataArchive with username/password
        /// </summary>
        [Fact]
        public void Connect2PIWithCredentials()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            //Please fill it with appropriate credentials. 
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", @"", EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);

            if (configurationRepository.GetConfiguration())
            {
                PIRepository piRespository = new PIRepository(_logPI);
                List<string> piDataArchiveNameList = new List<string>(configurationRepository.PIDataArchiveName.Split(',').ToList<string>());

                string piDataArchiveName = piDataArchiveNameList.First<string>();

                Assert.True(piRespository.Connect(configurationRepository.PIUser, configurationRepository.PIPassword, piDataArchiveName));
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }

        /// <summary>
        /// Try to connect with PI DataArchive with default user
        /// </summary>
        [Fact]
        public void Connect2PIWithDefaultUser()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            //Please fill it with appropriate credentials. 
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json", EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);
         
            if (configurationRepository.GetConfiguration())
            {
                PIRepository piRespository = new PIRepository(_logPI);
                List<string> piDataArchiveNameList = new List<string>(configurationRepository.PIDataArchiveName.Split(',').ToList<string>());

                //get the first server to try to connect
                string piDataArchiveName = piDataArchiveNameList.First<string>();
                                
                Assert.True(piRespository.Connect( piDataArchiveName));
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }

        /// <summary>
        /// Try to get data from PI DataArchive
        /// </summary>
        [Fact]
        public void GetDataFromPI()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            //Please fill it with appropriate credentials. 
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json", EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);

            if (configurationRepository.GetConfiguration())
            {
                PIRepository piRespository = new PIRepository(_logPI);
                List<string> piDataArchiveNameList = new List<string>(configurationRepository.PIDataArchiveName.Split(',').ToList<string>());

                string piDataArchiveName = piDataArchiveNameList.First<string>();

                if (piRespository.Connect(configurationRepository.PIUser, configurationRepository.PIPassword, piDataArchiveName))
                {
                    Assert.True(!string.IsNullOrEmpty(piRespository.GetDataFromArchive(piDataArchiveName, configurationRepository.PIAttributeDefinition)));
                }
                else
                {
                    Assert.True(false, "error connecting to pi");
                }
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }
    }
}
