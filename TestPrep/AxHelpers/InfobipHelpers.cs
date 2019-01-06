using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using RestSharp;

namespace TestPrep
{
    public class InfobipHelpers
    {
        //private string _username = "Bluma@1";
        //private string _password = "infobipP@ss1";
        private const string Url = "https://api.infobip.com/sms/1/text/multi";
        private const string ApiKey = "App 265c558cd29097194feffdd0d5657477-d39dbd1a-8066-4283-810f-3e570786b60a";
        private const string BasicToken = "Basic Qmx1bWFAMTppbmZvYmlwUEBzczE=";
        private const int Batchsize = 50;





        public class InfobipSmsMessage
        {
            public string from { get; set; }
            public string text { get; set; }
            public string to { get; set; }
        }
        public class InfobipSmsResponse
        {
            public string bulkId { get; set; }
            public List<InfobipSmsResponseMessage> messages { get; set; }
        }
        public class InfobipSmsResponseMessage
        {
            public InfobipMessageStatus status { get; set; }
            public string to { get; set; }
            public string smsCount { get; set; }
            public string messageId { get; set; }
        }
        public class InfobipMessageStatus
        {
            public long id { get; set; }
            public long groupId { get; set; }
            public string groupName { get; set; }
            public string name { get; set; }
            public string description { get; set; }
        }
    }
}