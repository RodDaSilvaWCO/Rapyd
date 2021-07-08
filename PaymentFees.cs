using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class PaymentFees
    {
        public TransactionFee transaction_fee { get; set; }
        /*
         * 
         {{
                            ""transaction_fee"": {{
                                ""calc_type"": ""gross"",
                                ""value"": 10,
                                ""fee_type"": ""percentage""
                            }}
         */
    }
}
