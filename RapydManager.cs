using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UnoSys.Api.Rapyd.Models;

namespace UnoSys.Api.Rapyd
{
    public static class RapydManager
    {
        #region Field Members
        private const string accessKey = "1153ED9EC362F8D7BB68";
        private const string secretKey = "ece8a94d1e4d4267e6214a985a38d4aa885ef514bf256fb15965bc19a51455bfbc3991872c033761";
        private const bool log = true;
        #endregion

        #region Public Implementation
        public static void GetListOfCountries()
        {
            // /data/countries
            try
            {
                string result = MakeRequest("GET", $"/v1/data/countries");
                if (log)
                {
                    Debug.Print($"WCOPapydManager.GetListOfCountries() - result={result.Substring(0,100) + "...."}");
                }
            }
            catch (Exception e)
            {
                Debug.Print("WCOPapydManager.GetListOfCountries(ERROR) - " + e.Message);
            }
        }

        public static void GetSupportedPayoutTypes(string payoutCurrency, string limit)
        {
            try
            {
                string result = MakeRequest("GET", $"/v1/payouts/supported_types?payout_currency={payoutCurrency}&limit={limit}");
                if (log)
                {
                    Debug.Print($"WCOPapydManager.GetSupportedPayoutTypes() - result={result}");
                }
            }
            catch (Exception e)
            {
                Debug.Print("PapydManager.GetSupportedPayoutTypes(ERROR) - " + e.Message);
            }
        }


        public static void GetPayoutRequredFields(string payoutMethodType, string beneficiaryCountry, string beneficiaryEntityType,
                                            string payoutAmount, string payoutCurrency, string senderCountry,
                                            string senderCurrency, string senderEntityType)
        {
            try
            {


                string result = MakeRequest("GET", $"/v1/payouts/{payoutMethodType}/details?beneficiary_country={beneficiaryCountry}&beneficiary_entity_type={beneficiaryEntityType}&payout_amount={payoutAmount}&payout_currency={payoutCurrency}&sender_country={senderCountry}&sender_currency={senderCurrency}&sender_entity_type={senderEntityType}");

                if (log)
                {
                    Debug.Print(result);
                }
            }
            catch (Exception e)
            {
                Debug.Print("RapydManager.GetPayoutRequiredFields(ERROR) - " + e.Message);
            }
        }


        public static string GetHostedPaymentCheckOutRedirectUrl(int dollaramount, int centsamount)
        {
            string redirect_url = null;
            try
            {

                HostedPaymentCheckOutRequest hpchr = new HostedPaymentCheckOutRequest
                {
                    amount = $"{dollaramount}{(centsamount == 0 ? "" : $".{centsamount}")}",
                    //ount = 5,
                    country = "US",
                    currency = "USD",
                    complete_payment_url = $"https://WorldComputer.org:{UnoSysApi.ComputeWorldComputerLocalPort()}/{dollaramount}{(centsamount == 0 ? "" : $".{centsamount}")}/SHELLAPP_REDIRECT_PARENT_SUCCESS",
                    complete_checkout_url = $"https://WorldComputer.org:{UnoSysApi.ComputeWorldComputerLocalPort()}/{dollaramount}{(centsamount == 0 ? "" : $".{centsamount}")}/SHELLAPP_REDIRECT_PARENT_SUCCESS",
                    cancel_checkout_url = $"https://WorldComputer.org:{UnoSysApi.ComputeWorldComputerLocalPort()}/SHELLAPP_REDIRECT_PARENT_CANCEL",
                    error_payment_url = $"https://WorldComputer.org:{UnoSysApi.ComputeWorldComputerLocalPort()}/SHELLAPP_REDIRECT_PARENT_ERROR",
                    payment_method_types_include = new string[] { "us_visa_card", "us_mastercard_card" },
                    payment_fees = new PaymentFees
                    {
                        transaction_fee = new TransactionFee
                        {
                            calc_type = "gross",
                            value = 4,
                            fee_type = "percentage"
                        }
                    }
                };
                var requestJson = JsonSerializer.Serialize<HostedPaymentCheckOutRequest>(hpchr);
                var responseJson = MakeRequest("POST", "/v1/checkout", requestJson);
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                var responseobj = root.GetProperty("status");
                var status = responseobj.GetProperty("status").GetString();
                if (status == "SUCCESS")
                {
                    var data = root.GetProperty("data");
                    redirect_url = data.GetProperty("redirect_url").GetString();
                }
            }
            catch (Exception ex)
            {

                Debug.Print($"RapydManager.GetHostedPaymentCheckOutRedirectUrl(ERROR) - {ex.Message}");
            }
            return redirect_url;
        }


        public static string PayoutToBeneficiary(string rapydsenderwalletid, string beneficiaryid, int dollaramount, int centsamount )
        {
            // "/v1/payouts"
            string transactionId = null;
            try
            {
                CreatePayoutRequest cpr = new CreatePayoutRequest
                {
                    ewallet = rapydsenderwalletid,
                    payout_amount = $"{dollaramount}.{centsamount}",
                    sender_currency = "USD",
                    sender_country = "US",
                    beneficiary_country = "US",
                    payout_currency = "USD",
                    sender_entity_type = "company",
                    beneficiary_entity_type = "individual",
                    beneficiary = beneficiaryid,
                    sender = new RapydSender
                    {
                        company_name = "World Computer Organization",
                        postcode = "L4C8Z9",
                        city = "Richmond Hill",
                        state = "Ontario",
                        address = "66 White Lodge Cres."
                    },
                    description = "Payout",
                    statement_descriptor = "World Computer ATTN Withdrawl"
                };
                var requestJson = JsonSerializer.Serialize<CreatePayoutRequest>(cpr);
                var responseJson = MakeRequest("POST", "/v1/payouts", requestJson);



                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                var statusObj = root.GetProperty("status");
                var status = statusObj.GetProperty("status").GetString();
                if (status == "SUCCESS")
                {
                    transactionId = statusObj.GetProperty("operation_id").GetString();
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"RapydManager.PayoutToBeneficiary(ERROR) - {ex.Message}");
            }
            return transactionId;
        }


        public static string GetHostedDisburseBeneficaryRedirectUrl( string idtype, string idvalue)
        {
            // /hosted/disburse/beneficiary
            string redirect_url = null;
            try
            {
                HostedDisburseBeneficaryRequest hdbr = new HostedDisburseBeneficaryRequest
                {
                    category = "card",
                    sender_entity_type = "company",
                    sender_country = "US",
                    beneficiary_entity_type = "individual",
                    cancel_url = $"https://www.WorldComputer.org:{UnoSysApi.ComputeWorldComputerLocalPort()}/SHELLAPP_CLOSE",
                    complete_url = $"https://www.WorldComputer.org:{UnoSysApi.ComputeWorldComputerLocalPort()}/SHELLAPP_CLOSE",  
                    beneficiary_optional_fields = new BeneficiaryOptionalFields
                    {
                        identification_type = idtype, // e.g.; "international_passport",
                        identification_value = idvalue, // e.g.; "123456789"
                    },
                    payout_method_types_include = new string[] { "us_visa_card", "us_mastercard_card" }
                };

                var requestJson = JsonSerializer.Serialize<HostedDisburseBeneficaryRequest>(hdbr);
                var responseJson = MakeRequest("POST", "/v1/hosted/disburse/beneficiary", requestJson);
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                var responseobj = root.GetProperty("status");
                var status = responseobj.GetProperty("status").GetString();
                if (status == "SUCCESS")
                {
                    var data = root.GetProperty("data");
                    redirect_url = data.GetProperty("redirect_url").GetString();
                }
                // HostedDisburseBeneficaryResponseData response = JsonSerializer.Deserialize<HostedDisburseBeneficaryResponseData>(responseJson);
            }
            catch (Exception ex)
            {
                Debug.Print($"RapydManager.HostedDisburseBeneficary(ERROR) - {ex.Message}");
            }
            return redirect_url;
        }

        public static string MakeRequest(string method, string urlPath, string body = null)
        {
            try
            {
                string httpMethod = method;
                Uri httpBaseURL = new Uri("https://sandboxapi.rapyd.net");
                string httpURLPath = urlPath;
                string httpBody = body;
                string salt = GenerateRandomString(8);
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                string signature = Sign(httpMethod, httpURLPath, salt, timestamp, httpBody);

                Uri httpRequestURL = new Uri(httpBaseURL, urlPath);
                WebRequest request = HttpWebRequest.Create(httpRequestURL);
                request.Method = httpMethod;
                request.ContentType = "application/json";
                request.Headers.Add("salt", salt);
                request.Headers.Add("timestamp", timestamp.ToString());
                request.Headers.Add("signature", signature);
                request.Headers.Add("access_key", accessKey);

                if (log)
                {
                    Debug.Print("web request method: " + httpMethod);
                    Debug.Print("web request url: " + httpRequestURL);
                    Debug.Print("web request body: " + httpBody);
                    Debug.Print("web request contentType: " + request.ContentType);
                    Debug.Print("web request salt: " + salt);
                    Debug.Print("web request timestamp: " + timestamp);
                    Debug.Print("web request signature: " + signature);
                    Debug.Print("web request access_key: " + accessKey);
                }

                return HttpRequest(request, body);
            }
            catch (Exception ex)
            {
                Debug.Print("Error generating request options: ", ex);
                throw;
            }
        }
        #endregion 

        #region Helpers
        private static string Sign(string method, string urlPath, string salt, long timestamp, string body)
        {

            try
            {
                string bodyString = String.Empty;
                if (!String.IsNullOrWhiteSpace(body))
                {
                    bodyString = body == "{}" ? "" : body;
                }

                string toSign = method.ToLower() + urlPath + salt + timestamp + accessKey + secretKey + bodyString;
                if (log)
                {
                    Debug.Print("\ntoSign: " + toSign);
                }

                UTF8Encoding encoding = new UTF8Encoding();
                byte[] secretKeyBytes = encoding.GetBytes(secretKey);
                byte[] signatureBytes = encoding.GetBytes(toSign);
                string signature = String.Empty;
                using (HMACSHA256 hmac = new HMACSHA256(secretKeyBytes))
                {
                    byte[] signatureHash = hmac.ComputeHash(signatureBytes);
                    string signatureHex = String.Concat(Array.ConvertAll(signatureHash, x => x.ToString("x2")));
                    signature = Convert.ToBase64String(encoding.GetBytes(signatureHex));
                }

                if (log)
                {
                    Debug.Print("signature: " + signature);
                }

                return signature;
            }
            catch (Exception)
            {
                Debug.Print("Error generating signature");
                throw;
            }

        }

        private static string GenerateRandomString(int size)
        {
            try
            {
                using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                {
                    byte[] randomBytes = new byte[size];
                    rng.GetBytes(randomBytes);
                    return String.Concat(Array.ConvertAll(randomBytes, x => x.ToString("x2")));
                }
            }
            catch (Exception)
            {
                Debug.Print("Error generating salt");
                throw;
            }
        }

        private static string HttpRequest(WebRequest request, string body)
        {
            string response = String.Empty;

            try
            {
                if (!String.IsNullOrWhiteSpace(body))
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        byte[] bodyBytes = encoding.GetBytes(body);
                        requestStream.Write(bodyBytes, 0, bodyBytes.Length);
                        requestStream.Close();
                    }
                }

                using (WebResponse webResponse = request.GetResponse())
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(responseStream))
                        {
                            response = streamReader.ReadToEnd();
                            if (log)
                            {
                                Debug.Print("web response:" + response.Length );
                                //Debug.Print("web response:" + (response.Length > 100 ? response.Substring(0,100) + "..." : response ));
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (StreamReader streamReader = new StreamReader(e.Response.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                    if (log)
                    {
                        Debug.Print("web response:" + response);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("Error occurred: " + e.Message);
            }

            return response;
        }
        #endregion
    }
}
