using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Implementations;
using OSIsoftPIAgentSOW.Services.Implementations;
using OSIsoftPIAgentSOW.Utils.Implementations;
using OSIsoftPIAgentSOW.Utils.Interfaces;
using System;
using Xunit;

namespace OSISoftAgentPIAgentTests
{
    public class IntegrationTests
    {
        private ILogger<AssetHubRepository> _logAssetHub;
        private ILogger<ConfigurationRepository> _logConfiguration;
        private ILogger<PIRepository> _logPI;
        private ILogger<FacadeService> _logFacadeService;
        private ILogger<HttpHelper> _logHttp;
        private IHttpHelper _httpHelper;

        public IntegrationTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            _logPI = factory.CreateLogger<PIRepository>();
            _logConfiguration = factory.CreateLogger<ConfigurationRepository>();
            _logAssetHub = factory.CreateLogger<AssetHubRepository>();
            _logFacadeService = factory.CreateLogger<FacadeService>();
            _logHttp = factory.CreateLogger<HttpHelper>();

            _httpHelper = new HttpHelper(_logHttp);
        }

        /// <summary>
        /// Test the facade Dotransfer method, the one that will connect to PI, retrieve data, 
        /// format it and than connect to asssethub, upload and commit the data
        /// </summary>
        [Fact]
        public void ServiceFacadeTest()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            //Please fill it with appropriate credentials. 
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json", EnvironmentVariableTarget.User);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);

            if (configurationRepository.GetConfiguration())
            {
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub, _httpHelper);
                PIRepository piRespository = new PIRepository(_logPI);
                FacadeService facadeService = new FacadeService(configurationRepository, assetHubRepository, piRespository, _logFacadeService);
                Assert.True(facadeService.DoTransfer());
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }
    }
}
