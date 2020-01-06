using System;
using System.Collections.Generic;
using OSIsoftPIAgentSOW.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSIsoftPIAgentSOW.Repositories.Interfaces
{
    public interface IPIRepository
    {

        bool Connect(string username, string password, string piDataArchiveName);

        bool Connect(string piDataArchiveName);

        string GetDataFromArchive(string piDataArchiveName, string piAttributeDefinition);

        void DisConnect();
    }
}
