using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSIsoftPIAgentSOW.Models
{
    public class AssetHubToken
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string authToken { get; set; }
        public string createdAt { get; set; }
    }
}
