using Microsoft.Extensions.Logging;
using OSIsoft.AF.PI;
using OSIsoftPIAgentSOW.Models;
using OSIsoftPIAgentSOW.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OSIsoftPIAgentSOW.Repositories.Implementations
{
    public class PIRepository : IPIRepository
    {
        private readonly ILogger _logHelper;
        private PIServer _piServer;

        public PIRepository(ILogger<PIRepository> logHelper)
        {
            _logHelper = logHelper;

        }

        /// <summary>
        /// create network credentials with username and password and use it to connect
        /// </summary>
        public bool Connect(string username, string password, string piDataArchiveName)
        {
            bool connectOK = false;

            try
            {
                NetworkCredential credential = new NetworkCredential(username, password);

                if (!(credential == null))
                {
                    _piServer = PIServer.FindPIServer(piDataArchiveName);

                    if (!(_piServer == null))
                    {
                        _piServer.Connect();
                        connectOK = true;
                    }
                    else
                    {
                        _logHelper.LogError(String.Format("{0} PI - Could not connect to {1} with user {2} ", DateTime.Now, piDataArchiveName, username));
                    }
                }
                else
                {
                    _logHelper.LogError(String.Format("{0} PI - Could not create network credentials with user {1} ", DateTime.Now, username));
                }
            }
            //TODO catch PIExcepion
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} PI Connect username {1}, piDataArchiveName {2}", DateTime.Now, username, piDataArchiveName));
            }

            return connectOK;
        }

        /// <summary>
        /// connect with current user
        /// </summary>
        public bool Connect(string piDataArchiveName)
        {

            bool connectOK = false;
            try
            {
                _piServer = PIServer.FindPIServer(piDataArchiveName);

                if (!(_piServer == null))
                {
                    _piServer.Connect();
                    connectOK = true;
                }
                else
                {
                    _logHelper.LogError(String.Format("{0} PI - Could not connect to {1} with current user", DateTime.Now, piDataArchiveName));
                }
            }
            //TODO catch PIExcepion
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} PI Connect piDataArchiveName {1}", DateTime.Now, piDataArchiveName));
            }

            return connectOK;
        }

        /// <summary>
        /// get the data from PI data archive and returns a string in CSV format, ready to be uploaded
        /// to asset hub
        /// </summary>
        public string GetDataFromArchive(string piDataArchiveName, string piAttributeDefinition)
        {
            StringBuilder bufferOutput = new StringBuilder();
            int count = 0;

            // create an attribute list from PIAttributeDefinition parameter
            string[] _desiredAttributeList = piAttributeDefinition.Split(',');
            bool pointsFound = false;
            try
            {
                //create the CSV header
                bufferOutput.AppendLine(piAttributeDefinition);

                //Get Points from PI
                IEnumerable<PIPoint> piPoints = PIPoint.FindPIPoints(_piServer, "*", null, null);

                using (var sequencePoints = piPoints.GetEnumerator())
                {
                    while (sequencePoints.MoveNext())
                    {
                        if (!pointsFound)
                        {
                            // used to verify if points where found
                            pointsFound = true;
                        }

                        PIPoint piPoint = sequencePoints.Current;
                        StringBuilder lineOfPoints = new StringBuilder("");

                        //try to get the desired attributes
                        piPoint.LoadAttributes(_desiredAttributeList);
                        IDictionary<string, object> dicPoint = piPoint.GetAttributes(_desiredAttributeList);

                        //check if desired the attributes have been retrieved in the current point
                        // if yes, add it to the line to be appended to the CSV
                        foreach (string attribute in _desiredAttributeList)
                        {
                            if (dicPoint.ContainsKey(attribute))
                            {
                                lineOfPoints.AppendFormat("{0}{1}", piPoint.GetAttribute(attribute), "," );
                            }
                            else
                            {
                                lineOfPoints.Append(",");
                            }
                        }
                        if (count % 100 == 0)
                        {   //log every 100th line of information
                            _logHelper.LogInformation(string.Format("{0} Retrieved {1} points so far...", DateTime.Now, (count+1).ToString() ));
                        }
                        count++;
                        string line = lineOfPoints.ToString();
                        //remove extra comma and appned to CSV string
                        bufferOutput.AppendLine(line.Substring(0, line.Length-1));
                    }
                }
            }
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} PI GetDataFromArchive PIDataArchiveName {1} piAttributeDefinition {2}", DateTime.Now, piDataArchiveName, piAttributeDefinition));
                pointsFound = false;
            }

            if (pointsFound)
            {
                _logHelper.LogInformation(string.Format("{0} Retrieved {1} points.", DateTime.Now, (count + 1).ToString()));
                return bufferOutput.ToString();
            }
            else
            {
                _logHelper.LogInformation(string.Format("{0} No points retrieved.", DateTime.Now));
                return "";
            }
            
        }

        /// <summary>
        /// disconnect from server
        /// </summary>
        public void DisConnect()
        {
            try
            {
                _piServer.Disconnect();
            }
            catch (Exception configException)
            {
                _logHelper.LogError(configException, String.Format("{0} PIRepository.Disconnect", DateTime.Now));
            }
        }
    }
}
