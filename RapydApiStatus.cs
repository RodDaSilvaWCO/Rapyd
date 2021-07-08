using System;
using System.Collections.Generic;
using System.Text;

namespace UnoSys.Api.Rapyd.Models
{
    public class RapydApiStatus
    {
        public string error_code { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public string operation_id { get; set; }

        // {"error_code":"","status":"SUCCESS","message":"","response_code":"","operation_id":"3a015383-edcd-42fc-b4e7-3f6350d02fea"}
    }
}
