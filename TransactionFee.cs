using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class TransactionFee
    {
        public string calc_type { get; set; }
        public decimal value { get; set; }
        public string fee_type { get; set; }
        /*
          ""calc_type"": ""gross"",
                                ""value"": 10,
                                ""fee_type"": ""percentage""
         * */
    }
}
