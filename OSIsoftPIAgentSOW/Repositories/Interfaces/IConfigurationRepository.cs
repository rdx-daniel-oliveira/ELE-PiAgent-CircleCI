using System.Collections.Generic;

namespace OSIsoftPIAgentSOW.Repositories.Interfaces
{
    public interface IConfigurationRepository
    {
        string PIDataArchiveName { get; set; }
        string PIUser { get; set; }
        string PIPassword { get; set; }
        string PIAttributeDefinition { get; set; }
        string EUser { get; set; }
        string EPassword { get; set; }
        string EOrgID { get; set; }
        string EDatasetID { get; set; }
        int EAgentIntervaMin { get; set; }
        int EAgentIntervaMiliSec { get; set; }
        string EBaseUrl { get; set; }

        bool GetConfiguration();

    }
}
