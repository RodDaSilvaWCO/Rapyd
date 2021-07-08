using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class CreatePayoutRequest
    {
        public string ewallet { get; set; }
        public string payout_amount { get; set; }
        public string sender_currency { get; set; }
        public string sender_country { get; set; }
        public string beneficiary_country { get; set; }
        public string payout_currency { get; set; }
        public string sender_entity_type { get; set; }
        public string beneficiary_entity_type { get; set; }
        public string beneficiary { get; set; }
        public RapydSender sender { get; set; }
        public string description { get; set; }
        public string statement_descriptor { get; set; }
        /*
          ""ewallet"": ""{rapydsenderwalletid}"",
                ""payout_amount"": {dollaramount}.{centsamount},
                ""sender_currency"": ""USD"",
                ""sender_country"": ""US"",
                ""beneficiary_country"": ""US"",
                ""payout_currency"": ""USD"",
                ""sender_entity_type"": ""company"",
                ""beneficiary_entity_type"": ""individual"",
                ""beneficiary"": ""beneficiary_99bf17493e67bbfbce50caac09e0c6f7"",  
                ""sender"": {{
                    ""company_name"" : ""World Computer Organization"",
                    ""postcode"" : ""L4C8Z9"",
                    ""city"" : ""Richmond Hill"",
                    ""state"" : ""Ontario"",
                    ""address"" : ""66 White Lodge Cres.""
                    }},  
                ""description"": ""Payout to card"",
                ""statement_descriptor"": ""World Computer ATTN Withdrawl""
         * */
    }
}
