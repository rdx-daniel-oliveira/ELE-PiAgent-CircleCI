using System;
using OSIsoftPIAgentSOW.Logging;

namespace OSIsoftPIAgentSOW.Configuration
{
    public class Configuration
    {
        public string PIAgentConfigFile { get; set; }
        public string PIDataArchiveName { get; set; }
        public string PIUser { get; set; }
        public string PIPassword { get; set; }
        public string PIAddress { get; set; }
        public string EUser { get; set; }
        public string EPassword { get; set; }
        public string EOrgID { get; set; }
        public string EDatasetID { get; set; }
        public int EAgentIntervaMin { get; set; }
        public string EBaseAdress { get; set; }
        public string EPostUrl { get; set; }
        public string ECommitUrl { get; set; }
        public bool ConfigOK { get; set; }


        public Configuration()
        {

           try
            {
                ConfigOK = true;
                string configError = "";
                //TODO implement parameters loading 

                //try environment variables, otherwise load from config
                //string g = Environment.GetEnvironmentVariable("");
                
                if (!ConfigOK)
                {
                    ErrorHandling.LogError(configError);
                }

            }
            catch (Exception configException)
            {
                ConfigOK = false;
                ErrorHandling.LogError(configException);
            }

        }
    }
}

