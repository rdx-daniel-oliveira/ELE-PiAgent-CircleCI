using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSIsoftPIAgentSOW.PIData.Models
{
    public class PIServer
    {
        public string PIAgentConfigFile { get; set; }
        public string PIDataArchiveName { get; set; }
        public string PIUser { get; set; }
        public string PIPassword { get; set; }
        public string PIAddress { get; set; }
    }
}
