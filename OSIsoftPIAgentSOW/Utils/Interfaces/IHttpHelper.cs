using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OSIsoftPIAgentSOW.Utils.Interfaces
{
    public interface IHttpHelper
    {
        string ServerReturn { get; set; }
        HttpStatusCode Status { get; set; }
        Task<bool> SendCommandToAssetHub(string postUrl, string contentType, string accept, string dataContent, Dictionary<string, string> requestHeaders, HttpStatusCode desiredStatusCode);
    }
}
