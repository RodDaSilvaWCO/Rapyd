using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class HostedDisburseBeneficaryResponseData
    {
        public string status { get; set; }
        public string cancel_url { get; set; }
        public string complete_url { get; set; }
        public string language { get; set; }
        public string merchant_color { get; set; }
        public string merchant_logo { get; set; }
        public string merchant_website { get; set; }


        public string merchant_language { get; set; }
        public MerchantCustomerSupport merchant_customer_support { get; set; }
        public string merchant_alias { get; set; }
        public string merchant_terms { get; set; }
        public string merchant_privacy_policy { get; set; }
        public long page_expiration { get; set; }
        public string redirect_url { get; set; }
        public string id { get; set; }

        public string category { get; set; }
        public string sender_entity_type { get; set; }
        public string sender_country { get; set; }
        public string merchant_reference_id { get; set; }
        public string beneficiary_entity_type { get; set; }
        public string beneficiary_country { get; set; }

        public string beneficiary_currency { get; set; }
        public string sender_currency { get; set; }
        public string beneficiary_id { get; set; }
        public string payout_method_type { get; set; }
        public bool beneficiary_validated { get; set; }

        public long timestamp { get; set; }
        public BeneficiaryOptionalFields beneficiary_optional_fields { get; set; }
        public string[] payout_method_types_include { get; set; }
        public string[] payout_method_types_exclude { get; set; }
        public long expiration { get; set; }
    }
}
