using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Implementations;
using OSIsoftPIAgentSOW.Utils.Implementations;
using OSIsoftPIAgentSOW.Utils.Interfaces;

using System;
using Xunit;

namespace OSISoftAgentPIAgentTests
{
    public class AssetHubRepositoryTests
    {
        private ILogger<AssetHubRepository> _logAssetHub;
        private ILogger<ConfigurationRepository> _logConfiguration;
        private ILogger<HttpHelper> _logHttp;

        private IHttpHelper _httpHelper;
        public AssetHubRepositoryTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            _logAssetHub = factory.CreateLogger<AssetHubRepository>();
            _logConfiguration = factory.CreateLogger<ConfigurationRepository>();
            _logHttp = factory.CreateLogger<HttpHelper>();

            _httpHelper = new HttpHelper(_logHttp);
        }

        /// <summary>
        /// Try to authenticate and get token from asset hub 
        /// </summary>
        [Fact]
        public void Connect2AssetHub()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            //Please fill it with appropriate credentials. 
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json", EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);

            if (configurationRepository.GetConfiguration())
            {
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub, _httpHelper);
                Assert.True(assetHubRepository.GetToken(configurationRepository.EUser, configurationRepository.EPassword, configurationRepository.EBaseUrl).Result);
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }

        /// <summary>
        /// Try to upload data to asset hub 
        /// </summary>
        [Fact]
        public void UploadData2AssetHub()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            //Please fill it with appropriate credentials. 
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json", EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);

            if (configurationRepository.GetConfiguration())
            {
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub, _httpHelper);

                bool assetHubTokenOk;

                if (!string.IsNullOrEmpty(configurationRepository.EStaticToken))
                {
                    assetHubRepository.SetToken(configurationRepository.EStaticToken);
                    assetHubTokenOk = true;
                }
                else
                {
                    assetHubTokenOk = assetHubRepository.GetToken(configurationRepository.EUser, configurationRepository.EPassword, configurationRepository.EBaseUrl).Result;
                }
                if (assetHubTokenOk)
                {
                    //mock PI data
                    string piData = @"name,pointsource,description,digitalset,engunits,exdesc,future,pointtype,ptclassname,sourcetag,archiving,compressing,span,step,zero,changedate,changer,creationdate,creator,pointid,instrumentag
,L,,,,,0,Float64,base,,1,1,100,0,0,01/07/2019 18:30:25,RDXPISANDBOX\zachary.burke,01/07/2019 18:30:25,RDXPISANDBOX\zachary.burke,887,";
                    string fileName = string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    Assert.True(assetHubRepository.StageFile(fileName, piData, configurationRepository.EBaseUrl, configurationRepository.EOrgID).Result);
                }
                else
                {
                    Assert.True(false, "error getting token");
                }
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }

        /// <summary>
        /// Try to commit uploaded data to asset hub 
        /// </summary>
        [Fact]
        public void CommitData2AssetHub()
        {
            //Load parameters from file to avoid misconfiguration from environment tests (the file is in the project's root folder).
            Environment.SetEnvironmentVariable("PI_AGENT_CONFIG_FILE", "agentconfiguration.json", EnvironmentVariableTarget.User);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(_logConfiguration);

            if (configurationRepository.GetConfiguration())
            {
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub, _httpHelper);

                bool assetHubTokenOk;

                if (!string.IsNullOrEmpty(configurationRepository.EStaticToken))
                {
                    assetHubRepository.SetToken(configurationRepository.EStaticToken);
                    assetHubTokenOk = true;
                }
                else
                {
                    assetHubTokenOk = assetHubRepository.GetToken(configurationRepository.EUser, configurationRepository.EPassword, configurationRepository.EBaseUrl).Result;
                }
                if (assetHubTokenOk)
                {
                    Assert.True(assetHubRepository.CommitData(configurationRepository.EBaseUrl, configurationRepository.EOrgID, configurationRepository.EDatasetID, "DummyFileName").Result);
                }
                else
                {
                    Assert.True(false, "error getting token");
                }
            }
            else
            {
                Assert.True(false, "error getting config");
            }
        }
    }
}
