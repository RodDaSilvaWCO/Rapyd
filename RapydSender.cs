using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class RapydSender
    {
        public string company_name { get; set; }
        public string postcode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string address { get; set; }
        /*
         * 
          ""sender"": {{
                    ""company_name"" : ""World Computer Organization"",
                    ""postcode"" : ""L4C8Z9"",
                    ""city"" : ""Richmond Hill"",
                    ""state"" : ""Ontario"",
                    ""address"" : ""66 White Lodge Cres.""
                    }},  
         * */
    }
}
