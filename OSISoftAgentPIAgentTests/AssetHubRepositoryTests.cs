using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Implementations;
using System;
using Xunit;

namespace OSISoftAgentPIAgentTests
{
    public class AssetHubRepositoryTests
    {
        private ILogger<AssetHubRepository> _logAssetHub;
        private ILogger<ConfigurationRepository> _logConfiguration;

        public AssetHubRepositoryTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            _logAssetHub = factory.CreateLogger<AssetHubRepository>();
            _logConfiguration = factory.CreateLogger<ConfigurationRepository>();
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
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub);
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
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub);
                if (assetHubRepository.GetToken(configurationRepository.EUser, configurationRepository.EPassword, configurationRepository.EBaseUrl).Result)
                {
                    //mock PI data
                    string piData = @"name,pointsource,description,digitalset,engunits,exdesc,future,pointtype,ptclassname,sourcetag,archiving,compressing,span,step,zero,changedate,changer,creationdate,creator,pointid,instrumentag
,L,,,,,0,Float64,base,,1,1,100,0,0,01/07/2019 18:30:25,RDXPISANDBOX\zachary.burke,01/07/2019 18:30:25,RDXPISANDBOX\zachary.burke,887,";
                    string fileName =  string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    Assert.True(assetHubRepository.StageFile(fileName, piData, configurationRepository.EBaseUrl, configurationRepository.EOrgID, configurationRepository.EDatasetID).Result);
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
                AssetHubRepository assetHubRepository = new AssetHubRepository(_logAssetHub);
                if (assetHubRepository.GetToken(configurationRepository.EUser, configurationRepository.EPassword, configurationRepository.EBaseUrl).Result)
                {
                    //mock PI data
                    string piData = @"name,pointsource,description,digitalset,engunits,exdesc,future,pointtype,ptclassname,sourcetag,archiving,compressing,span,step,zero,changedate,changer,creationdate,creator,pointid,instrumentag
,L,,,,,0,Float64,base,,1,1,100,0,0,01/07/2019 18:30:25,RDXPISANDBOX\zachary.burke,01/07/2019 18:30:25,RDXPISANDBOX\zachary.burke,887,";
                    string fileName = string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    if (assetHubRepository.StageFile(fileName, piData, configurationRepository.EBaseUrl, configurationRepository.EOrgID, configurationRepository.EDatasetID).Result)
                    {
                        Assert.True(assetHubRepository.CommitData(configurationRepository.EBaseUrl, configurationRepository.EOrgID, configurationRepository.EDatasetID).Result);
                    }
                    else
                    {
                        Assert.True(false, "error staging file");
                    }
                
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
