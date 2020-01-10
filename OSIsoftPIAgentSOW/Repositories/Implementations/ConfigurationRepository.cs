using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Repositories.Interfaces;
using System;
using System.Text;

namespace OSIsoftPIAgentSOW.Repositories.Implementations
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        public string PIDataArchiveName { get; set; }
        public string PIUser { get; set; }
        public string PIPassword { get; set; }
        public string PIAttributeDefinition {get; set;}        
        public string EUser { get; set; }
        public string EPassword { get; set; }
        public string EOrgID { get; set; }
        public string EDatasetID { get; set; }
        public int EAgentIntervaMin { get; set; }
        public int EAgentIntervaMiliSec { get; set; }
        public string EBaseUrl { get; set; }
        public string EStaticToken { get; set; }

        private readonly ILogger _logHelper;

        public ConfigurationRepository(ILogger<ConfigurationRepository> logHelper)
        {
            _logHelper = logHelper;
        }

        public bool GetConfiguration()
        {
            bool configOK = true;
            string configError = "";
            string pIAgentConfigFile = "";
            try
            {
                pIAgentConfigFile = GetEnvironmentVariable("PI_AGENT_CONFIG_FILE");

                if (string.IsNullOrEmpty(pIAgentConfigFile))
                {
                    GetParamsFromEnvironment();
                }
                else
                {
                    GetParamsFromFile(pIAgentConfigFile);
                }
                
                configError = TestConfiguration();

                if (!string.IsNullOrEmpty(configError))
                {
                    configOK = false;
                    _logHelper.LogError(string.Format("{0} GetConfiguration {1}", DateTime.Now, configError));
                }
            }
            catch (Exception configException)
            {
                _logHelper.LogError(configException, string.Format("{0} GetConfiguration", DateTime.Now ));
                configOK = false;
            }

            return configOK;
        }

        private void GetParamsFromEnvironment()
        {
            this.PIDataArchiveName = GetEnvironmentVariable("PI_DATA_ARCHIVE_NAME");
            this.PIUser = GetEnvironmentVariable("PI_USER");
            this.PIPassword = GetEnvironmentVariable("PI_PASSWORD");
            this.PIAttributeDefinition = GetEnvironmentVariable("PI_ATTRIBUTE_DEFINITION");
            this.EUser = GetEnvironmentVariable("E_USER");
            this.EPassword = GetEnvironmentVariable("E_PASSWORD");
            this.EOrgID = GetEnvironmentVariable("E_ORG_ID");
            this.EDatasetID = GetEnvironmentVariable("E_DATASET_ID");
            int intervalTimer = 0;
            if (int.TryParse(GetEnvironmentVariable("E_AGENT_INTERVAL_MIN"), out intervalTimer))
            {
                this.EAgentIntervaMin = intervalTimer;
                this.EAgentIntervaMiliSec = EAgentIntervaMin * 60 * 1000;
            }
            this.EBaseUrl = GetEnvironmentVariable("E_BASE_URL");
            this.EStaticToken = GetEnvironmentVariable("E_STATIC_TOKEN");
        }

        private void GetParamsFromFile(string fileName)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddJsonFile(fileName, optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = configurationBuilder.Build();

            var settings = configuration.GetSection("AgentConfiguration");

            this.PIDataArchiveName = settings["PI_DATA_ARCHIVE_NAME"];
            this.PIUser = settings["PI_USER"];
            this.PIPassword = settings["PI_PASSWORD"];
            this.PIAttributeDefinition = settings["PI_ATTRIBUTE_DEFINITION"];
            this.EUser = settings["E_USER"];
            this.EPassword = settings["E_PASSWORD"];
            this.EOrgID = settings["E_ORG_ID"];
            this.EDatasetID = settings["E_DATASET_ID"];
            int intervalTimer = 0;
            if (int.TryParse(settings["E_AGENT_INTERVAL_MIN"], out intervalTimer))
            {
                this.EAgentIntervaMin = intervalTimer;
                this.EAgentIntervaMiliSec = EAgentIntervaMin * 60 * 1000;
            }
            this.EAgentIntervaMin = int.Parse(settings["E_AGENT_INTERVAL_MIN"]);
            this.EAgentIntervaMiliSec = EAgentIntervaMin * 60 * 1000;
            this.EBaseUrl = settings["E_BASE_URL"];
            this.EStaticToken = settings["E_STATIC_TOKEN"];
        }

        private string TestConfiguration()
        {
            StringBuilder errorMessage = new StringBuilder("");

            if (string.IsNullOrEmpty(this.PIDataArchiveName))
            {
                errorMessage.AppendLine("Configuration Error: PI_DATA_ARCHIVE_NAME is empty");
            }

            if (string.IsNullOrEmpty(this.PIAttributeDefinition))
            {
                errorMessage.AppendLine("Configuration Error: PI_ATTRIBUTE_DEFINITION is empty");
            }

            if (string.IsNullOrEmpty(this.EUser))
            {
                errorMessage.AppendLine("Configuration Error: E_USER is empty");
            }

            if (string.IsNullOrEmpty(this.EPassword))
            {
                errorMessage.AppendLine("Configuration Error: E_PASSWORD is empty");
            }

            if (string.IsNullOrEmpty(this.EOrgID))
            {
                errorMessage.AppendLine("Configuration Error: E_ORG_ID is empty");
            }

            if (string.IsNullOrEmpty(this.EDatasetID))
            {
                errorMessage.AppendLine("Configuration Error: E_DATASET_ID is empty");
            }

            if (this.EAgentIntervaMin <= 0)
            {
                errorMessage.AppendLine("Configuration Error: E_AGENT_INTERVAL_MIN is invalid");
            }

            if (string.IsNullOrEmpty(this.EBaseUrl))
            {
                errorMessage.AppendLine("Configuration Error: E_BASE_URL is empty");
            }

            return errorMessage.ToString();
        }

        private string GetEnvironmentVariable(string variable)
        {
            string variableValue = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(variableValue))
            {
                variableValue = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine);
                if (string.IsNullOrEmpty(variableValue))
                {
                    variableValue = Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Process);
                }
            }

            return variableValue;
        }
    }
}
