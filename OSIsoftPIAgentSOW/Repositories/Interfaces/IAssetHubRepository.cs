using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSIsoftPIAgentSOW.Repositories.Interfaces
{
    public interface IAssetHubRepository
    {

        Task<bool> GetToken(string eUser, string ePassword, string EBaseUrl);
        void SetToken(string token);

        Task<bool> StageFile(string filename, string dataContent, string eBaseUrl, string eOrgID);

        Task<bool> CommitData(string eBaseUrl, string eOrgID, string eDatasetID, string fileName);
    }
}
