using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Models;
using OSIsoftPIAgentSOW.Repositories.Interfaces;
using OSIsoftPIAgentSOW.Utils.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OSIsoftPIAgentSOW.Repositories.Implementations
{
    public class AssetHubRepository : IAssetHubRepository
    {
        private readonly ILogger _logHelper;
        private AssetHubToken _token;
        private string _groupID;
        private IHttpHelper _httpHelper;

        public AssetHubRepository(ILogger<AssetHubRepository> logHelper, IHttpHelper httpHelper)
        {
            _logHelper = logHelper;
            _groupID = "";
            _httpHelper = httpHelper;
        }

        public void SetToken(string token)
        {
            _token = new AssetHubToken();
            _token.authToken = token;
        }

        public async Task<bool> GetToken(string eUser, string ePassword, string eBaseUrl)
        {
            bool tokenOK = false;
            string postUrl = "";

            try
            {
                string credentials = "{{\"email\": \"{0}\",\"password\":\"{1}\"}}";
                postUrl = (string.Format("{0}sessions", eBaseUrl));

                Dictionary<string, string> requestHeaders = new Dictionary<string, string>();

                tokenOK = await _httpHelper.SendCommandToAssetHub(postUrl, "application/json; charset=UTF-8", "application/json", credentials, requestHeaders, HttpStatusCode.OK);
            }
            
            catch (Exception configException)
            {
                _logHelper.LogError(configException, string.Format("{0}  AssetHub.GetToken username {1}, EPostUrl {2}", DateTime.Now, eUser, postUrl));
            }

            return tokenOK;
        }

      
        public async Task<bool> StageFile(string filename, string dataContent, string eBaseUrl, string eOrgID)
        {
            bool postOK = false;
            string postUrl = "";

           _groupID = "";

            try
            {
                postUrl = string.Format("{0}stage/file?name={1}&format=csv&convertToParquet=false", eBaseUrl, filename);
                Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
                requestHeaders.Add("x-auth-token", _token.authToken);
                requestHeaders.Add("x-organization-id", eOrgID);

                postOK = await _httpHelper.SendCommandToAssetHub(postUrl, "application/octet-stream", "", dataContent, requestHeaders, HttpStatusCode.OK);

                if (postOK)
                {
                    _groupID = _httpHelper.ServerReturn;
                }
            }
           
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} AssetHub.StageFile EPostUrl {1} eOrgID {2} ", DateTime.Now, postUrl, eOrgID));
            }

            return postOK;
        }


     

        public async Task<bool> CommitData(string eBaseUrl, string eOrgID, string eDatasetID, string fileName)
        {
            bool commitOK = false;
            string commitCommand = "";
            string postUrl = "";

            try
            {
               commitCommand = string.Format("{{ \"commands\": [ {{ \"$type\": \"append\", \"name\": \"{0}\", \"group\": \"{1}\",\"cause\": []  }}] }}", fileName, _groupID);
                               
                postUrl = string.Format("{0}dataset/{1}/commit", eBaseUrl, eDatasetID);

                Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
                requestHeaders.Add("x-auth-token", _token.authToken);
                requestHeaders.Add("x-organization-id", eOrgID);

                commitOK = await _httpHelper.SendCommandToAssetHub(postUrl, "application/json; charset=UTF-8", "application/json", commitCommand, requestHeaders, HttpStatusCode.Accepted);

            }
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} AssetHub.CommitData EPostUrl {1} dataset {2} eOrgID {3} Commit Command {4}", DateTime.Now, postUrl, eDatasetID, eOrgID, commitCommand));
            }

            return commitOK;
        }
  
    }
}
