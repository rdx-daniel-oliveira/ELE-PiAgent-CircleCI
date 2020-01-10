using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OSIsoftPIAgentSOW.Utils.Interfaces;

namespace OSIsoftPIAgentSOW.Utils.Implementations
{
    public class HttpHelper: IHttpHelper
    {
        public string ServerReturn { get; set; }
        public HttpStatusCode Status { get; set; }

        private readonly ILogger _logHelper;
        public HttpHelper(ILogger<HttpHelper> logHelper)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            _logHelper = logHelper;
        }

        public async Task<bool> SendCommandToAssetHub(string postUrl, string contentType, string accept, string dataContent, Dictionary<string, string> requestHeaders, HttpStatusCode desiredStatusCode)
        {
            bool postOk = false;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(postUrl);
                req.Method = "POST";
                req.ContentType = contentType;

                if (!string.IsNullOrEmpty(accept))
                {
                    req.Accept = accept;
                }

                foreach (KeyValuePair<string, string> entry in requestHeaders)
                {
                    req.Headers.Add(entry.Key, entry.Value);
                }

                byte[] postBytes = Encoding.UTF8.GetBytes(dataContent);
                req.ContentLength = postBytes.Length;

                Stream postStream = req.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Stream stream = resp.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream))
                {
                    ServerReturn = await reader.ReadToEndAsync();
                }
                stream.Close();

                if (resp.StatusCode == desiredStatusCode)
                {
                    postOk = true;
                }
                Status = resp.StatusCode;
            }
            catch (WebException webException)
            {
                try
                {
 
                    Stream stream = webException.Response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        ServerReturn = await reader.ReadToEndAsync();
                    }
                    Status = (HttpStatusCode)webException.Status;
                }
                catch { }
                _logHelper.LogError(webException, String.Format("{0} EPostUrl {1} eOrgI Message {2}", DateTime.Now, postUrl, ServerReturn));
            }
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} EPostUrl {1} ", DateTime.Now, postUrl));
                Status = HttpStatusCode.InternalServerError;
            }
            return postOk;
        }
    }
}
