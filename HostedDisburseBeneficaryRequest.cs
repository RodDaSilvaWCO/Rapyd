using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class HostedDisburseBeneficaryRequest
    {
        public string category { get; set; }
        public string sender_entity_type { get; set; }
        public string sender_country { get; set; }
        public string beneficiary_entity_type { get; set; }
        public string cancel_url { get; set; }
        public string complete_url { get; set; }
        public BeneficiaryOptionalFields beneficiary_optional_fields { get; set; }
        public string[] payout_method_types_include { get; set; }

        /*
          {{
                ""category"": ""card"",
                ""sender_entity_type"": ""company"",
                ""sender_country"": ""US"", 
                ""beneficiary_entity_type"": ""individual"",
                ""cancel_url"": ""https://www.WorldComputer.org:51930"",
                ""complete_url"": ""https://www.WorldComputer.org:51930"",
                ""beneficiary_optional_fields"": {{
                    ""identification_type"": ""international_passport"",
                    ""identification_value"": ""123456789""
                }},
                ""payout_method_types_include"": [
                        ""us_visa_card"",
                        ""us_mastercard_card""
                        ]
            }}";
         * */
    }

   
}
