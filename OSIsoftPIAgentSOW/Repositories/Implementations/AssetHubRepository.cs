using OSIsoftPIAgentSOW.Models;
using OSIsoftPIAgentSOW.Repositories.Interfaces;
using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.IO;

namespace OSIsoftPIAgentSOW.Repositories.Implementations
{
    public class AssetHubRepository : IAssetHubRepository
    {
        private readonly ILogger _logHelper;

        private AssetHubToken _token;
        private readonly HttpClient _client;

        public AssetHubRepository(ILogger<AssetHubRepository> logHelper)
        {
           _logHelper = logHelper;
            _client = new HttpClient();
        }

        public async Task<bool> GetToken(string eUser, string ePassword, string eBaseUrl)
        {
            bool tokenOK = false;
            _token = new AssetHubToken();
            string serverReturn = "";

            try
            {
                string credentials = "{{\"email\": \"{0}\",\"password\":\"{1}\"}}";

                HttpContent httpContent = new StringContent(string.Format(credentials, eUser, ePassword), Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await _client.PostAsync(string.Format("{0}sessions", eBaseUrl), httpContent);

                serverReturn = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _token = JsonConvert.DeserializeObject<AssetHubToken>(serverReturn.Trim());
                    tokenOK = true;
                }
                else
                {
                    if (response.StatusCode != HttpStatusCode.NotFound)
                    {
                        _logHelper.LogError(string.Format("{0} AssetHub.GetToken StatusCode {1} ServerReturn {2}", DateTime.Now, response.StatusCode.ToString(),serverReturn));
                    }
                }

            }
            catch (Exception configException)
            {
               _logHelper.LogError(configException, string.Format("{0}  AssetHub.GetToken username {1}, Server return {2}", DateTime.Now, eUser, serverReturn));
            }

            return tokenOK;
        }

       
        public async Task<bool> StageFile(string filename, string dataContent, string eBaseUrl, string eOrgID, string eDatasetID)
        {
            bool postOK = false;

            try
            {
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("x-auth-token", _token.authToken);
             // _client.DefaultRequestHeaders.Add("xOrganizationId", eOrgID);
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}stage/file?name={1}&format=csv&convertToParquet=true&groupId=1", eBaseUrl, filename));
                request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(dataContent));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content.Headers.Add("xOrganizationId", eOrgID);

                HttpResponseMessage response = await _client.SendAsync(request);
                string serverReturn = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    postOK = true;
                }
                else
                {
                    _logHelper.LogError(string.Format("{0} AssetHub.StageFile StatusCode {1} ServerReturn {2}", DateTime.Now, response.StatusCode.ToString(), serverReturn));
                }
            }
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} AssetHub.StageFile EPostUrl {1} eOrgID {2} eDatasetID {3}", DateTime.Now, eBaseUrl, eOrgID, eDatasetID));
            }

            return postOK;
        }

        public async Task<bool> CommitData(string eBaseUrl, string eOrgID, string eDatasetID)
        {
            bool commitOK = false;
          
            try
            {
                string commitCommand = "{\"commands\":[{}]}";

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _client.DefaultRequestHeaders.Add("x-auth-token", _token.authToken);
                _client.DefaultRequestHeaders.Add("xOrganizationId", eOrgID);

                HttpContent httpContent = new StringContent(commitCommand, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(string.Format("{0}dataset/{1}/commit", eBaseUrl, eDatasetID), httpContent);

                string serverReturn = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == (HttpStatusCode)202)
                {
                    commitOK = true;
                }
                else
                {
                    _logHelper.LogError(string.Format("{0} AssetHub.CommitData StatusCode {1} ServerReturn {2}", DateTime.Now, response.StatusCode.ToString(), serverReturn));
                }
              
            }
            catch (Exception configException)
            {
               _logHelper.LogError(configException, String.Format("{0} AssetHub.CommitData eCommitUrl {1} dataset {2} eOrgID {3}", DateTime.Now, eBaseUrl, eDatasetID, eOrgID));
            }

            return commitOK;
        }
    }
}
