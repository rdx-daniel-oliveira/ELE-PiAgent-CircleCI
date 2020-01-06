using Microsoft.Extensions.Logging;
using OSIsoftPIAgentSOW.Models;
using OSIsoftPIAgentSOW.Repositories.Interfaces;
using OSIsoftPIAgentSOW.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace OSIsoftPIAgentSOW.Services.Implementations
{
    /// <summary>
    /// service facade that will orchestrate the process
    /// </summary>

    public class FacadeService : IFacadeService
    {
        private IConfigurationRepository _configRepository;
        private IAssetHubRepository _assetHubRepository;
        private IPIRepository _pIRepository;
        private readonly ILogger _logHelper;

        // used to prevent reentrancy
        private bool _inTimerSemaphore;


        public FacadeService(IConfigurationRepository configurationRepository,
            IAssetHubRepository assetHubRepository,
            IPIRepository pIRepository,
            ILogger<FacadeService> logHelper)
        {
            _configRepository = configurationRepository;
            _assetHubRepository = assetHubRepository;
            _pIRepository = pIRepository;
            _logHelper = logHelper;
        }

        /// <summary>
        /// Load the configuration settings into repository objects that will manipulate data
        /// Start the timer that will execute the procecactchss 
        /// </summary>
        public bool SetUp()
        {
            bool setupOk = false;
            try
            {
                // retrieve parameters and create the timer that will get the job done
                _logHelper.LogInformation(string.Format("{0} Loading Configurations", DateTime.Now));
                if (_configRepository.GetConfiguration())
                {
                    _logHelper.LogInformation(string.Format("{0} Configurations loaded", DateTime.Now));
                    
                    Timer aTimer = new Timer(_configRepository.EAgentIntervaMiliSec);
                    aTimer.Elapsed += TimerHandler;
                    aTimer.AutoReset = true;
                    aTimer.Enabled = true;
                   
                    _logHelper.LogInformation(string.Format("{0} Timer created with {1} miliseconds interval. Waiting trigger...", DateTime.Now, _configRepository.EAgentIntervaMiliSec));
                    setupOk = true;
                }
            }
            catch (Exception exception)
            {
                _logHelper.LogError(exception, string.Format("{0} FacadeService DoTransfer", DateTime.Now));
            }
            return setupOk;
        }

        /// <summary>
        /// Timer handler that controls the process
        /// </summary>
        private void TimerHandler(Object source, ElapsedEventArgs e)
        {
            if (!_inTimerSemaphore)
            {
                _inTimerSemaphore = true;
                DoTransfer();
                _inTimerSemaphore = false;
            }
        }

        /// <summary>
        /// Get PI Data, convert it to CSV
        /// Upload to asset hub
        /// </summary>
        public bool DoTransfer()
        {
            bool transferCompletedWithSuccess = false;

            _logHelper.LogInformation(string.Format("Starting transfer {0}", DateTime.Now));
            DateTime startTranferTime = DateTime.Now;

            try
            {
                //create a list of servers from the comma separated PI data archive name parameter
                List<string> piDataArchiveNameList = new List<string>(_configRepository.PIDataArchiveName.Split(',').ToList<string>());

                // for each pi data archive, connect, fetch the data and upload to asset hub 
                foreach (string piDataArchiveName in piDataArchiveNameList)
                {
                    bool connectToPI;

                    _logHelper.LogInformation(string.Format("{0} Connecting to PI DataArchive {1}", DateTime.Now, piDataArchiveName));
                    //check to decide wether to connect with username/password and try to connect
                    if (string.IsNullOrEmpty(_configRepository.PIUser))
                    {
                        connectToPI = _pIRepository.Connect(piDataArchiveName);
                    }
                    else
                    {
                        connectToPI = _pIRepository.Connect(_configRepository.PIUser, _configRepository.PIPassword, piDataArchiveName);
                    }

                    if (connectToPI)
                    {
                        _logHelper.LogInformation(string.Format("{0} Fetching data from PI DataArchive {1}", DateTime.Now, piDataArchiveName));

                        // go to PI and fecth the data
                        string piData = _pIRepository.GetDataFromArchive(piDataArchiveName, _configRepository.PIAttributeDefinition);

                        if (!string.IsNullOrEmpty(piData))
                        {

                            _logHelper.LogInformation(string.Format("{0} Getting token from asset hub", DateTime.Now));
                            // try to connect to assethub
                            if (_assetHubRepository.GetToken(_configRepository.EUser, _configRepository.EPassword, _configRepository.EBaseUrl).Result)
                            {
                                // try to upload the file

                                _logHelper.LogInformation(string.Format ("{0} Token OK. Uploading data to asset hub ", DateTime.Now));

                                string fileName = string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMddHHmmss"));
                                if (_assetHubRepository.StageFile(fileName, piData, _configRepository.EBaseUrl, _configRepository.EOrgID, _configRepository.EDatasetID).Result)
                                {
                                    _logHelper.LogInformation(string.Format("{0} Upload OK. Committing data in asset hub", DateTime.Now));
                                    // try to commit 
                                    if (_assetHubRepository.CommitData(_configRepository.EBaseUrl, _configRepository.EOrgID, _configRepository.EDatasetID).Result)
                                    {
                                        transferCompletedWithSuccess = true;
                                        _logHelper.LogInformation(string.Format("{0} Commit OK", DateTime.Now));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logHelper.LogError(exception, string.Format("{0} FacadeService DoTransfer", DateTime.Now));
            }

            TimeSpan timeSpent = (DateTime.Now - startTranferTime);
            _logHelper.LogInformation("{0} End Transfer - total time elapsed {1} minutes and {2} seconds. Waiting next timer trigger...", DateTime.Now, timeSpent.Minutes, timeSpent.Seconds);

            return transferCompletedWithSuccess;
        }
    }
}
