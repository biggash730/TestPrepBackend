using System.Net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using smsghapi_dotnet_v2.Smsgh;
using ApiHost = hubtelapi_dotnet_v1.Hubtel.ApiHost;
using BasicAuth = hubtelapi_dotnet_v1.Hubtel.BasicAuth;

namespace TestPrep.AxHelpers
{
    public class SmsGhHelpers
    {
        //https://api.hubtel.com/vend
        private const bool SecuredConnection = true;
        private const string ClientId = "evvvcmln";
        private const string ClientSecret = "dotjfmvo";
        private const string Hostname = "https://api.hubtel.com";
        private const string EndPoint = "vend/mobilemoney";
        private const string ApiToken = "bf3df8d8-7083-4935-95e5-d1d0f9b12f2c";
        // Test Path : usp/test
        // Live Path: usp
        private static readonly string ContextPath = "usp";

        private static readonly ApiHost Host = new ApiHost
        {
            SecuredConnection = SecuredConnection,
            ContextPath = ContextPath,
            EnabledConsoleLog = true,
            Hostname = Hostname,
            Auth = new BasicAuth(ClientId, ClientSecret)
        };

        public BrokerResponse TransferToMobileMoneyWithVendCallBack(string receiverPhoneNo, string receiverName, string provider, double amount, string foreignId, string callbackurl)
        {
            var client = new RestClient(Hostname)
            {
                Authenticator = new HttpBasicAuthenticator(ClientId, ClientSecret)
            };
            var request = new RestRequest(EndPoint, Method.POST)
            {
                RequestFormat = DataFormat.Json
                
            };
            //request.RequestFormat = DataFormat.Json;
            //request.Method = Method.POST;
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("Accept", "application/json");
            //request.AddHeader("Authorization", "Basic " + );
            //request.AddHeader("Content-Type", "application/json");
            //request.AddParameter("receiverPhone", receiverPhoneNo);
            //request.AddParameter("receiverName", receiverName);
            //request.AddParameter("amount", amount);
            //request.AddParameter("provider", provider);
            //request.AddParameter("callbackUrl", callbackurl);
            //request.AddParameter("foreignId", foreignId);
            //request.AddParameter("sender", "Lending Sqr");
            //request.AddParameter("token", ApiToken);
            var obj = new MobileMoneyObject
            {
                amount = amount,
                callbackUrl = callbackurl,
                receiverName = receiverName,
                receiverPhone = receiverPhoneNo,
                provider = provider,
                foreignId = foreignId,
                token = ApiToken
            };
            request.AddBody(obj);
            var res = client.Execute(request);
            return res.StatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<BrokerResponse>(res.Content) : new BrokerResponse();
        }

        //public BrokerResponse TransferToMobileMoneyWithVend(string receiverPhoneNo, string receiverName, string provider, double amount, string foreignId)
        //{
        //    //Money Transfer Provider either mtn/airtel
        //    return _brokerClient.TransferToMobileMoney(receiverPhoneNo, receiverName, amount, provider.ToUpper(), "LendingSqr",
        //        foreignId);
        //}

        //public BrokerResponse ReceiveFromMobileMoney(string receiverPhoneNo, string receiverName, string provider, double amount, string foreignId, string token = "")
        //{
        //    var resource = $"/{"mobilemoney"}/";
        //    var obj = new MobileMoneyObject
        //    {
        //        receiverPhone = receiverPhoneNo,
        //        receiverName = receiverName,
        //        foreignId = foreignId,
        //        amount = amount,
        //        provider = provider.ToUpper(),
        //        token = ApiToken,
        //        receiveToken = token
        //    };

        //    return BrokerApi.SendPostBrokerRequest(resource, obj);
        //}


    }

    public class MobileMoneyObject
    {
        public string receiverPhone { get; set; }
        public string receiverName { get; set; } //person you're receiving money from
        public double amount { get; set; }
        public string foreignId { get; set; }
        public string token { get; set; }
        public string provider { get; set; } // MTN, AIRTEL, VODAFONE, TIGO
        public string sender { get; set; } = "LendingSqr";
        public string callbackUrl { get; set; }
    }
}