using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class HostedDisburseBeneficiaryResponse
    {
        public RapydApiStatus status { get; set; }
        public HostedDisburseBeneficaryResponseData data { get; set; }

        /*
         {"status":{"error_code":"","status":"SUCCESS","message":"","response_code":"","operation_id":"3a015383-edcd-42fc-b4e7-3f6350d02fea"},
        "data":{"status":"NEW","cancel_url":"https://www.WorldComputer.org:51930","complete_url":"https://www.WorldComputer.org:51930","language":"","merchant_color":"d0922c","merchant_logo":"https://sboxiconslib.rapyd.net/merchant-logos/ohpc_c44b34ddf6532a5bec8c9bbad58d3e37.png","merchant_website":"https://www.WorldComputer.org","merchant_language":"en","merchant_customer_support":{"url":"https://WorldComputer.org","email":"Inquiries@WorldComputer.org"},"merchant_alias":"N/A","merchant_terms":"","merchant_privacy_policy":"","page_expiration":1626551796,"redirect_url":"https://sandboxhosted.rapyd.net/disburse/beneficiary?token=hp_ben_2af2ff8ea99c82e05059866b6d604696","id":"hp_ben_2af2ff8ea99c82e05059866b6d604696","category":"card","sender_entity_type":"company","sender_country":"US","merchant_reference_id":null,"beneficiary_entity_type":"individual","beneficiary_country":null,"beneficiary_currency":null,"sender_currency":"USD","beneficiary_id":null,"payout_method_type":null,"beneficiary_validated":false,"timestamp":1625342196,"beneficiary_optional_fields":{"last_name":null,"first_name":null,"company_name":null,"identification_type":"international_passport","identification_value":"123456789"},"payout_method_types_include":["us_visa_card","us_mastercard_card"],"payout_method_types_exclude":null,"expiration":1626551796}}
         * 
         */
    }
}
