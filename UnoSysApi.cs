using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;
using UnoSys.Core.Api;
using System.Security.Principal;
using UnoSys.Kernel.ApiTypes;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Security.Authentication;

namespace UnoSys.Api
{
    public  class UnoSysApi : IDisposable
    {
        #region Field Members
        private const string API_REQUEST_CONTENT_TYPE = "application/json";
        private const string API_CONTROLLER = "FirmwareApi";
        private HttpClient httpClient;
        private string unoSysApiUrl = null;
        private const string unoSysNodeApiUrlTemplate = "https://localhost:{0}/";
        //private const string localWorldComputerUrlTemplate = "https://worldcomputer.org:{0}/Identity/Account/Register";
        private const string worldComputerRegistryKeTemplatey = @"SOFTWARE\World Computer Organization\{0}";
        private string filePath = null;
        private string appAccessToken = null;
        static public string UserAccessToken = null;
        static public string OSSID = null;
        static string dynamicPort = null;
        static string dynamicApiPort = null;
        private const int WORLD_COMPUTER_BASE_PORT = 49928;
        static public string WCORapydWalletId = "<WCO Rapyd Treasury Wallet Id place holder>";  // %TODO% - retrieve this from an encrypted file off of the World Computer vDrive
        private HttpClientHandler clientHandler = null;
        #endregion

        #region Constructor
        //public UnoSysApi( string filepath )
        //{
        //    filePath = filepath;
        //    //unoSysApiUrl = ComputeUnoSysApiUrlForFilePath(filepath);
        //    unoSysApiUrl = ComputeWorldComputerLocalUrl();
        //    if( string.IsNullOrEmpty(unoSysApiUrl) )
        //    {
        //        throw new UnauthorizedAccessException("A World Computer Node is not currently running on this machine.");
        //    }    
        //    httpClient = new HttpClient();
        //    httpClient.BaseAddress = new Uri(unoSysApiUrl);
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        //}

        public UnoSysApi()
        {
            //unoSysApiUrl = ComputeUnoSysApiUrlForFilePath(filepath);
            unoSysApiUrl =  ComputeWorldComputerLocalApiUrl();
            if (string.IsNullOrEmpty(unoSysApiUrl))
            {
                throw new UnauthorizedAccessException("A World Computer Node is not currently running on this machine.");
            }
            clientHandler = GetHandler();
            httpClient = new HttpClient(clientHandler);
            httpClient.BaseAddress = new Uri(unoSysApiUrl);
//            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        private static HttpClientHandler GetHandler()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.SslProtocols = SslProtocols.None;
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            return handler;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
            httpClient = null;
            clientHandler?.Dispose();
            clientHandler = null;
        }
        #endregion


        #region Public Instance Apis
        public async Task<int> SetPublishPermissions( string fileName, string osSidToAllow, uint processIdToAllow, string processNameToAllow )
        {
            int SDID = Int32.MaxValue;
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("FileName", typeof(string), fileName));
            pList.Add(new ApiParameter("OsSidToAllow", typeof(string), osSidToAllow));
            pList.Add(new ApiParameter("ProcessIdToAllow", typeof(uint), processIdToAllow.ToString()));
            pList.Add(new ApiParameter("ProcessNameToAllow", typeof(string), processNameToAllow));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.SetPublishPermissions, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
            if( response.StatusCode != HttpStatusCode.OK)
            {
                Debug.Print($"SetPublishPermissions ERROR - {response.StatusCode} ");
            }
            else
            {
                SDID = await ApiParameterValidation.GetValidResult<int>(response).ConfigureAwait(false);
            }
            return SDID;
        }

        public async Task ResetPublishPermissions(string fileName, string osSidToAllow, uint processIdToAllow, string processNameToAllow)
        {
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("FileName", typeof(string), fileName));
            pList.Add(new ApiParameter("OsSidToAllow", typeof(string), osSidToAllow));
            pList.Add(new ApiParameter("ProcessIdToAllow", typeof(uint), processIdToAllow.ToString()));
            pList.Add(new ApiParameter("ProcessNameToAllow", typeof(string), processNameToAllow));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.ResetPublishPermissions, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        }

        //public async Task TestMoveDirectory(string filePath)
        //{
        //    List<ApiParameter> pList = new List<ApiParameter>();
        //    pList.Add(new ApiParameter("filePath", typeof(string), filePath));
        //    var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TestCreateDir, pList);
        //    var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        //}


        public async Task<PublishedContentReceipt> GetPublishedContentStatus( string userAccessToken, string ossid )
         {
            PublishedContentReceipt result = new PublishedContentReceipt();  // Default in case of error
            try
            {
                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("UserAccessToken", typeof(string), userAccessToken));
                pList.Add(new ApiParameter("LocalFilePath", typeof(string), filePath));
                //WindowsIdentity id = System.Security.Principal.WindowsIdentity.GetCurrent();
                //pList.Add(new ApiParameter("UserSid", typeof(string), id.User.ToString()));
                pList.Add(new ApiParameter("UserSid", typeof(string), ossid));
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPublishedContentStatus, pList);
                // Post the ApiRequest
                var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    var pcReceiptJson = await ApiParameterValidation.GetValidResult<string>(response).ConfigureAwait(false);
                    result = (PublishedContentReceipt)JsonConvert.DeserializeObject(pcReceiptJson, typeof(PublishedContentReceipt));
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPublishedContentStatus(): StatusCode={response.StatusCode} ");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPublishedContentStatus(): {ex.ToString()}");
            }
            return result;
         }


        //public async Task<bool> ValidSSIDAccessTokenAsync(string ssidAccessToken )
        //{
        //    bool isValidResult = false;
        //    if (!string.IsNullOrEmpty(ssidAccessToken))
        //    {
        //        try
        //        {
        //            // Create parameter list for call
        //            List<ApiParameter> pList = new List<ApiParameter>();
        //            pList.Add(new ApiParameter("SsidAccessToken", typeof(string), ssidAccessToken));
        //            // Create ApiRequest with parameter list
        //            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.ValidateSSIDAccessToken, pList);
        //            // Post the ApiRequest
        //            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                // Retrieve result of call
        //                isValidResult = await ApiParameterValidation.GetValidResult<bool>(response).ConfigureAwait(false);

        //            }
        //            else
        //            {
        //                Debug.Print($"Non-200 StatusCode in ValidSSIDAccessTokenAsync(): StatusCode={response.StatusCode} ");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.Print($"Error in ValidSSIDAccessTokenAsync(): {ex.ToString()}");
        //        }
        //    }
        //    return isValidResult;
        //}

        public async Task<string> GetUserSSIDAccessTokenAsync(string appAccessToken, string username, string password)
        {
            UserAccessToken = null;
            //appAccessToken = await AppLoginAsync().ConfigureAwait(false);
            if(!string.IsNullOrEmpty(appAccessToken) )
            {
                UserAccessToken = await UserLoginAsync(appAccessToken, username, password).ConfigureAwait(false);
            }
            return UserAccessToken;
        }


        public async Task<PublishedContentReceipt> PublishedContent( string userAccessToken, PublishedContentDefinition pcdef )
        {
            PublishedContentReceipt pcReceipt = null;
            try
            {
                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("UserAccessToken", typeof(string), userAccessToken));
                pList.Add(new ApiParameter("PublishedContentDefinitionJson", typeof(string), JsonConvert.SerializeObject(pcdef)));
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.PublishContent, pList);
                // Post the ApiRequest
                var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    var pcReceiptJson = await ApiParameterValidation.GetValidResult<string>(response).ConfigureAwait(false);
                    pcReceipt = (PublishedContentReceipt)JsonConvert.DeserializeObject(pcReceiptJson, typeof(PublishedContentReceipt));
                    Debug.Print($"PublishedContent returned successfull!!!!!!!!! - Title={pcReceipt.PublishedContentDefinition.Title}" );
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in PublishedContent(): StatusCode={response.StatusCode} ");
                }
            }
            catch (Exception)
            {

                throw;
            }
            return await Task.FromResult<PublishedContentReceipt>( pcReceipt );
        }


        public async Task<string> TreasuryTokenTransferAsync(string appAccessToken, string ssid, bool fromTreasury, uint amount)
        {
            string trxReceipt = "0.0.0";
            // Create parameter list for call
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("AppAccessToken", typeof(string), appAccessToken));
            pList.Add(new ApiParameter("UserSid", typeof(string), ssid));
            pList.Add(new ApiParameter("FromTreasury", typeof(bool), JsonConvert.SerializeObject(fromTreasury)));
            pList.Add(new ApiParameter("Amount", typeof(uint), JsonConvert.SerializeObject(amount)));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TreasuryTransfer, pList);
            // Post the ApiRequest
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Retrieve result of call
                trxReceipt = await ApiParameterValidation.GetValidResult<string>(response).ConfigureAwait(false);
                if (trxReceipt == null )
                {
                    trxReceipt = "0.0.0";  
                }
            }
            else
            {
                Debug.Print($"Non-200 StatusCode in TreasuryTokenTransferAsync(): StatusCode={response.StatusCode} ");
            }
            return await Task.FromResult<string>(trxReceipt);
        }



        public async Task<string> ComputePublishedFilename(PublishedContentDefinition pcDef)
        {
            string publishedFileName = null;
            try
            {

                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("UserAccessToken", typeof(string), UserAccessToken));
                pList.Add(new ApiParameter("DRMAccessType", typeof(int), ((int)pcDef.DRMPolicy.AccessType).ToString()));
                pList.Add(new ApiParameter("ContentType", typeof(int), pcDef.ContentType.ToString()));
                pList.Add(new ApiParameter("Category", typeof(int), pcDef.Category.ToString()));
                pList.Add(new ApiParameter("Topic", typeof(int), pcDef.Topic.ToString()));
                pList.Add(new ApiParameter("Language", typeof(int), pcDef.Language.ToString()));
                pList.Add(new ApiParameter("Library", typeof(int), pcDef.Library.ToString()));
                pList.Add(new ApiParameter("Channel", typeof(int), pcDef.Channel.ToString()));
                pList.Add(new ApiParameter("LocalFilePath", typeof(string), pcDef.LocalFilePath));
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.ComputePublishedFileName, pList);
                // Post the ApiRequest
                var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    publishedFileName = await ApiParameterValidation.GetValidResult<string>(response).ConfigureAwait(false);
                    Debug.Print($"ComputePublishedFilename returned successfull!!!!!!!!! - publishedFileName={publishedFileName}");
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in ComputePublishedFilename(): StatusCode={response.StatusCode} ");
                }
            }
            catch (Exception)
            {

                throw;
            }
            return await Task.FromResult<string>(publishedFileName);
        }


        public string ComputeWorldComputerLocalApiUrl()
        {
            string worldCOmputerUrl = "";
            try
            {
                // Step #1: Read running processes to see if WorldComputer exe is running
                List<string> candidateProcesses = new List<string>();
                foreach (var p in Process.GetProcesses())
                {
                    // e.g.; WorldComputer-79994A67356EED76
                    var processRootName = p.ProcessName.ToUpper();
                    if (processRootName.Length == 30 && processRootName.Substring(0, 14) == "WORLDCOMPUTER-")
                    {
                        candidateProcesses.Add(p.ProcessName);
                    }
                }
                if (candidateProcesses.Count == 1)  // Only can proceed if exactly one World Computer Node running...
                {
                    // #Step 2: Read Registry for found World Computer Node to get vDrive letter
                    string vDriveLetter = null;
                    try
                    {
                        // Read "X" Key which should hold the preferred drive letter for the particular WorldComputer Node
                        vDriveLetter = ReadWorldComputerVDriveLetter(string.Format(worldComputerRegistryKeTemplatey, candidateProcesses[0]));
                    }
                    catch (Exception /*ex*/)
                    {
                        // NOP
                    }
                    if (!string.IsNullOrEmpty(vDriveLetter))
                    {
                        // Compute the dynamic UnoSys.Node API URL
                        var baseport = ComputeBasePort(WORLD_COMPUTER_BASE_PORT, vDriveLetter.Substring(0, 1));
                        dynamicPort = (baseport).ToString();
                        dynamicApiPort = (baseport + 2).ToString();
                        worldCOmputerUrl = string.Format(unoSysNodeApiUrlTemplate, dynamicApiPort);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error - {ex.ToString()}");
                // NOP
            }
            return worldCOmputerUrl;
        }



        public static string ComputeWorldComputerLocalPort()
        {
            if (dynamicPort == null)
            {
                try
                {
                    // Step #1: Read running processes to see if WorldComputer exe is running
                    List<string> candidateProcesses = new List<string>();
                    foreach (var p in Process.GetProcesses())
                    {
                        // e.g.; WorldComputer-79994A67356EED76
                        var processRootName = p.ProcessName.ToUpper();
                        if (processRootName.Length == 30 && processRootName.Substring(0, 14) == "WORLDCOMPUTER-")
                        {
                            candidateProcesses.Add(p.ProcessName);
                        }
                    }
                    if (candidateProcesses.Count == 1)  // Only can proceed if exactly one World Computer Node running...
                    {
                        // #Step 2: Read Registry for found World Computer Node to get vDrive letter
                        string vDriveLetter = null;
                        try
                        {
                            // Read "X" Key which should hold the preferred drive letter for the particular WorldComputer Node
                            vDriveLetter = ReadWorldComputerVDriveLetter(string.Format(worldComputerRegistryKeTemplatey, candidateProcesses[0]));
                        }
                        catch (Exception /*ex*/)
                        {
                            // NOP
                        }
                        if (!string.IsNullOrEmpty(vDriveLetter))
                        {
                            // Compute the dynamic UnoSys.Node API URL
                            dynamicPort = (ComputeBasePort(WORLD_COMPUTER_BASE_PORT, vDriveLetter.Substring(0, 1))).ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print($"Error - {ex.ToString()}");
                    // NOP
                }
            }
            return dynamicPort;
        }

        public static string ComputeWorldComputerApiPort( string baseWcoPort)
        {
            if (dynamicApiPort == null)
            {
               dynamicApiPort = (int.Parse(ComputeWorldComputerLocalPort()) + 2).ToString();
            }
            return dynamicApiPort;
        }


        //private string ComputeUnoSysApiUrlForFilePath(string filePath)
        //{
        //    if (!string.IsNullOrEmpty(filePath))
        //    {
        //        try
        //        {
        //            // Step #1: Read running processes to see if WorldComputer exe is running
        //            List<string> candidateProcesses = new List<string>();
        //            foreach (var p in Process.GetProcesses())
        //            {
        //                // e.g.; WorldComputer-79994A67356EED76
        //                var processRootName = p.ProcessName.ToUpper();
        //                if (processRootName.Length == 30 && processRootName.Substring(0, 14) == "WORLDCOMPUTER-")
        //                {
        //                    candidateProcesses.Add(p.ProcessName);
        //                }
        //            }
        //            if (candidateProcesses.Count == 1)  // Only can proceed if exactly one World Computer Node running...
        //            {
        //                // #Step 2: Read Registry for found World Computer Node to get vDrive letter
        //                string vDriveLetter = null;
        //                try
        //                {
        //                    // Read "X" Key which should hold the preferred drive letter for the particular WorldComputer Node
        //                    vDriveLetter = ReadWorldComputerVDriveLetter(string.Format(worldComputerRegistryKeTemplatey, candidateProcesses[0]));
        //                }
        //                catch (Exception /*ex*/)
        //                {
        //                    // NOP
        //                }
        //                if (!string.IsNullOrEmpty(vDriveLetter))
        //                {
        //                    // Compute the dynamic UnoSys.Node API URL
        //                    string dynamicPort = (ComputeBasePort(WORLD_COMPUTER_BASE_PORT, vDriveLetter.Substring(0, 1)) + 2).ToString();
        //                    unoSysApiUrl = string.Format(unoSysNodeApiUrlTemplate, dynamicPort);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.Print($"Error - {ex.ToString()}");
        //            // NOP
        //        }
        //    }
        //    return unoSysApiUrl;
        //}


        public bool GetSSIDInfoFromSSIDAccessToken( string SSIDAccessToken, out long ssid, out string ssidName, out string defaultDisplayName )
        {
            bool success = false;
            ssid = (long)int.MaxValue;  // NOTE:  in the long range of long.MinValue - 0 - long.MaxValue, (long)int.MaxValue == long.MaxValue
            ssidName = null;
            defaultDisplayName = null;
            try
            {
                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("UserSessionToken", typeof(string), SSIDAccessToken));
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetSSIDInfo, pList);
                // Post the ApiRequest
                var response = httpClient.PostAsync(API_CONTROLLER, content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    //var result = ApiResponse.GetApiResponseOutputs(response).Result;
                    var expectedOutputs = new List<ApiParameterValidation>();
                    expectedOutputs.Add(new ApiParameterValidation("SSIDName", typeof(string)));
                    expectedOutputs.Add(new ApiParameterValidation("DefaultDisplayName", typeof(string)));
                    expectedOutputs.Add(new ApiParameterValidation("SSID", typeof(long)));
                    var result = ApiResponse.GetValidOutputs(expectedOutputs, response).Result;
                    ssid = result.GetValue<long>("SSID");
                    ssidName = result.GetValue<string>("SSIDName");
                    defaultDisplayName = result.GetValue<string>("DefaultDisplayName");
                    success = true;
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetSSIDInfoFromSSIDAccessToken(): StatusCode={response.StatusCode} ");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetSSIDInfoFromSSIDAccessToken(): {ex.ToString()}");
            }
            return success;
        }

        public bool GetPrimaryUserOSSIDFromUserSSID(string SSIDAccessToken, out string ossid )
        {
            bool success = false;
            ossid = null;
            try
            {
                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("UserSessionToken", typeof(string), SSIDAccessToken));
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPrimaryUserOSSIDFromUserSSID, pList);
                // Post the ApiRequest
                var response = httpClient.PostAsync(API_CONTROLLER, content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    ossid = ApiParameterValidation.GetValidResult<string>(response).Result;
                    success = !string.IsNullOrEmpty(ossid);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPrimaryUserOSSIDFromUserSSID(): StatusCode={response.StatusCode} ");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPrimaryUserOSSIDFromUserSSID(): {ex.ToString()}");
            }
            return success;
        }


        public async Task<Dictionary<long, string>> GetPublishedContentTypes()
        {
            Dictionary<long, string> result = null;
            try
            {
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPublishedContentTypes);
                // Post the ApiRequest
                var response = await PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    List<ApiParameterValidation> validOutputs = new List<ApiParameterValidation>();
                    validOutputs.Add(new ApiParameterValidation("Keys", typeof(List<long>)));
                    validOutputs.Add(new ApiParameterValidation("Values", typeof(List<string>)));
                    // Retrieve the received validated arguments from the request 
                    ApiTypedValues outputs = await ApiResponse.GetValidOutputs(validOutputs, response).ConfigureAwait(false);
                    var keys = outputs.GetValue<List<long>>("Keys");
                    var values = outputs.GetValue<List<string>>("Values");
                    ListsToDict(keys, values, out result);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPublishedContentTypes(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPublishedContentTypes(): {ex.ToString()}");
            }
            return result;
        }



        public async Task<Dictionary<long, string>> GetPublishedContentTypeHints()
        {
            Dictionary<long, string> result = null;
            try
            {
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPublishedContentTypeHints);
                // Post the ApiRequest
                var response = await PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    List<ApiParameterValidation> validOutputs = new List<ApiParameterValidation>();
                    validOutputs.Add(new ApiParameterValidation("Keys", typeof(List<long>)));
                    validOutputs.Add(new ApiParameterValidation("Values", typeof(List<string>)));
                    // Retrieve the received validated arguments from the request 
                    ApiTypedValues outputs = await ApiResponse.GetValidOutputs(validOutputs, response).ConfigureAwait(false);
                    var keys = outputs.GetValue<List<long>>("Keys");
                    var values = outputs.GetValue<List<string>>("Values");
                    ListsToDict(keys, values, out result);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPublishedContentTypeHints(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPublishedContentTypeHints(): {ex.ToString()}");
            }
            return result;
        }


        public async Task<Dictionary<long, string>> GetPublishedContentCategories()
        {
            Dictionary<long, string> result = null;
            try
            {
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPublishedCategories);
                // Post the ApiRequest
                var response = await PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    List<ApiParameterValidation> validOutputs = new List<ApiParameterValidation>();
                    validOutputs.Add(new ApiParameterValidation("Keys", typeof(List<long>)));
                    validOutputs.Add(new ApiParameterValidation("Values", typeof(List<string>)));
                    // Retrieve the received validated arguments from the request 
                    ApiTypedValues outputs = await ApiResponse.GetValidOutputs(validOutputs, response).ConfigureAwait(false);
                    var keys = outputs.GetValue<List<long>>("Keys");
                    var values = outputs.GetValue<List<string>>("Values");
                    ListsToDict(keys, values, out result);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPublishedContentCategories(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPublishedContentCategories(): {ex.ToString()}");
            }
            return result;
        }


        public async Task<Dictionary<long, string>> GetPublishedContentTopics()
        {
            Dictionary<long, string> result = null;
            try
            {
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPublishedTopics);
                // Post the ApiRequest
                var response = await PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    List<ApiParameterValidation> validOutputs = new List<ApiParameterValidation>();
                    validOutputs.Add(new ApiParameterValidation("Keys", typeof(List<long>)));
                    validOutputs.Add(new ApiParameterValidation("Values", typeof(List<string>)));
                    // Retrieve the received validated arguments from the request 
                    ApiTypedValues outputs = await ApiResponse.GetValidOutputs(validOutputs, response).ConfigureAwait(false);
                    var keys = outputs.GetValue<List<long>>("Keys");
                    var values = outputs.GetValue<List<string>>("Values");
                    ListsToDict(keys, values, out result);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPublishedContentTopics(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPublishedContentTopics(): {ex.ToString()}");
            }
            return result;
        }


        public async Task<Dictionary<long, string>> GetPublishedContentLanguages()
        {
            Dictionary<long, string> result = null;
            try
            {
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.GetPublishedLanguages);
                // Post the ApiRequest
                var response = await PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    List<ApiParameterValidation> validOutputs = new List<ApiParameterValidation>();
                    validOutputs.Add(new ApiParameterValidation("Keys", typeof(List<long>)));
                    validOutputs.Add(new ApiParameterValidation("Values", typeof(List<string>)));
                    // Retrieve the received validated arguments from the request 
                    ApiTypedValues outputs = await ApiResponse.GetValidOutputs(validOutputs, response).ConfigureAwait(false);
                    var keys = outputs.GetValue<List<long>>("Keys");
                    var values = outputs.GetValue<List<string>>("Values");
                    ListsToDict(keys, values, out result);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in GetPublishedContentLanguages(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in GetPublishedContentLanguages(): {ex.ToString()}");
            }
            return result;
        }

        #region Test Stuff
        public async Task TestRead(string filePath)
        {
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("filePath", typeof(string), filePath));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TestRead, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        }
        public async Task TestWrite(string filePath)
        {
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("filePath", typeof(string), filePath));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TestWrite, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        }

        public async Task TestDelete(string filePath)
        {
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("filePath", typeof(string), filePath));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TestDelete, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        }
        public async Task TestCreateDirectory(string filePath)
        {
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("filePath", typeof(string), filePath));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TestCreateDir, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        }

        public async Task TestDeleteDirectory(string filePath)
        {
            List<ApiParameter> pList = new List<ApiParameter>();
            pList.Add(new ApiParameter("filePath", typeof(string), filePath));
            var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.TestDeleteDir, pList);
            var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
        }
        #endregion



        #endregion


        #region Helpers
       

        public async Task<string> AppLoginAsync(string appId, string appKey )
        {
            string result = "";
            try
            {
                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("AppId", typeof(string), appId));
                pList.Add(new ApiParameter("AppKey", typeof(string), appKey));
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.AppLogin, pList);
                // Post the ApiRequest
                //var response = await PostAsync(API_CONTROLLER, content, isAnonymous: true).ConfigureAwait(false);
                var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    result = await ApiParameterValidation.GetValidResult<string>(response).ConfigureAwait(false);
                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in AppLoginAsync(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in AppLoginAsync(): {ex.ToString()}");
            }
            return result;
        }


        private async Task<string> UserLoginAsync( string appAccessToken, string username, string password )
        {
            string result = "";
            try
            {
                // Create parameter list for call
                List<ApiParameter> pList = new List<ApiParameter>();
                pList.Add(new ApiParameter("AppSessionToken", typeof(string), appAccessToken));
                pList.Add(new ApiParameter("UserName", typeof(string), username));
                pList.Add(new ApiParameter("Password", typeof(string), password));
                // Create ApiRequest with parameter list
                var content = ApiRequest.CreateApiRequest((int)ApiIdentifier.UserLogin, pList);
                // Post the ApiRequest
                var response = await httpClient.PostAsync(API_CONTROLLER, content).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Retrieve result of call
                    result = await ApiParameterValidation.GetValidResult<string>(response).ConfigureAwait(false);

                }
                else
                {
                    Debug.Print($"Non-200 StatusCode in UserLoginAsync(): StatusCode={response.StatusCode} {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in UserLoginAsync(): {ex.ToString()}");
            }
            return result;
        }




        private async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            HttpResponseMessage response = null;
            response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
            return response;
        }


        static private string ReadWorldComputerVDriveLetter(string registryKey)
        {
            string vDriveLetter = null;
            try
            {
                using (RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key = localKey.OpenSubKey(registryKey, false))
                    {
                        vDriveLetter = Encoding.ASCII.GetString((byte[])key.GetValue("X"));
                    }
                }
            }
            catch (Exception)
            {
                // NOP - best effort
            }
            return vDriveLetter;
        }

        static private int ComputeBasePort(int basePort, string driveLetter)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(driveLetter);
            return basePort + (26 * asciiBytes[0]);
        }

        static private void ListsToDict<K, V>(List<K> keys, List<V> vals, out Dictionary<K, V> dict)
        {
            dict = null;
            if (keys != null && vals != null && keys.Count == vals.Count && keys.Count > 0)
            {
                dict = new Dictionary<K, V>(keys.Count);
                for (int i = 0; i < keys.Count; i++)
                {
                    dict.Add(keys[i], vals[i]);
                }
            }
        }



        #endregion


    }
}
