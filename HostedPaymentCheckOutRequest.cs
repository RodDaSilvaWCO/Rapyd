using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class HostedPaymentCheckOutRequest
    {
        public string amount { get; set; }
        public string country { get; set; }
        public string currency { get; set; }
        public string complete_payment_url { get; set; }
        public string complete_checkout_url { get; set; }
        public string cancel_checkout_url { get; set; }
        public string error_payment_url { get; set; }
        public string[] payment_method_types_include { get; set; }
        public PaymentFees payment_fees { get; set; }
        /*
         * 
          ""amount"": ""{dollaramount}.{centsamount}"",
                    ""country"": ""US"",
                    ""currency"": ""USD"",
                    ""complete_payment_url"": ""https://WorldComputer.org:51930"",
                    ""complete_checkout_url"": ""https://WorldComputer.org:51930"",
                    ""cancel_checkout_url"": ""https://WorldComputer.org:51930"",
                    ""error_payment_url"": ""https://WorldComputer.org:51930"",
                    ""payment_method_types_include"": [          
                        ""us_visa_card"",
                        ""us_mastercard_card""
                        ],
                    ""payment_fees"": {{
                            ""transaction_fee"": {{
                                ""calc_type"": ""gross"",
                                ""value"": 10,
                                ""fee_type"": ""percentage""
                            }}
                     }}
         * */
    }
}
