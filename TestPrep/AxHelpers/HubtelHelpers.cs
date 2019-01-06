using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using TestPrep.Models;

namespace TestPrep.AxHelpers
{
    public class HubtelHelpers
    {
        private const string AccountNumber = "HM2711170028";
        private const string ClientId = "jodztjhl";
        private const string ClientSecret = "figwttzg";
        private const string Hostname = "https://api.hubtel.com/v1/merchantaccount";
        private const string ReceiveEndPoint = "/merchants/{MerchantAccountNumber}/receive/mobilemoney";

        private const string CheckTransactionEndPoint =
            "/merchants/{MerchantAccountNumber}/transactions/status?hubtelTransactionId={TransactionId}";

        private const string ConfirmTopupTransactionEndPoint =
            "/merchants/{MerchantAccountNumber}/transactions/status?networkTransactionId={TransactionId}";

        private const string ReceiveCallbackUrl = "http://nmprep.azurewebsites.net/api/public/receivemobilemoneycallback";

        public static void ReceiveMobileMoney(long id)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        var trans =
                            db.subscriptionPayments.FirstOrDefault(
                                x => x.Id == id && x.Status == SubscriptionPaymentStatus.Pending);
                        if (trans == null) return;
                        var sub = db.Subscriptions.First(x => x.Id == trans.SubscriptionId);
                        var user = db.Users.First(x => x.Id == sub.UserId);
                        var endPoint = ReceiveEndPoint.Replace("{MerchantAccountNumber}", AccountNumber);

                        var client = new RestClient(Hostname)
                        {
                            Authenticator = new HttpBasicAuthenticator(ClientId, ClientSecret)
                        };
                        var request = new RestRequest(endPoint, Method.POST)
                        {
                            RequestFormat = DataFormat.Json
                        };

                        request.AddParameter("CustomerName", user.Name);
                        request.AddParameter("CustomerMsisdn", trans.Number);
                        request.AddParameter("CustomerEmail", user.Email);
                        request.AddParameter("Channel", trans.Network);
                        request.AddParameter("Amount", trans.Amount);
                        request.AddParameter("PrimaryCallbackURL", ReceiveCallbackUrl);
                        request.AddParameter("SecondaryCallbackURL", ReceiveCallbackUrl);
                        if (trans.Network.Contains("vodafone")) request.AddParameter("Token", trans.Token);
                        request.AddParameter("Description", "Subscription Payment");
                        request.AddParameter("ClientReference", trans.Reference);
                        
                        var res = client.Execute(request);
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            var result = JsonConvert.DeserializeObject<MomoResponse1>(res.Content);
                            if (result.ResponseCode == "0001")
                            {
                                trans.Status = SubscriptionPaymentStatus.Processing;
                                trans.TransactionId = result.Data.TransactionId;
                            }
                            else
                            {
                                trans.Status = SubscriptionPaymentStatus.Failed;
                                sub.Status = SubscriptionStatus.PaymentFailed;
                            }
                        }
                        else
                        {
                            trans.Status = SubscriptionPaymentStatus.Failed;
                            sub.Status = SubscriptionStatus.PaymentFailed;
                        }
                        trans.Response = res.Content;
                        db.SaveChanges();
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                WebHelpers.ProcessException(ex);
            }
        }

        //public static void CheckReceiveMobileMoneyStatus(long id)
        //{
        //    try
        //    {
        //        using (var db = new AppDbContext())
        //        {
        //            using (var transaction = db.Database.BeginTransaction())
        //            {
        //                var trans =
        //                    db.subscriptionPayments.FirstOrDefault(
        //                        x => x.Id == id && x.Status == SubscriptionPaymentStatus.Processing);
        //                if (trans == null) return;

        //                var user = db.Users.First(x => x.UserName == trans.Subscription.CreatedBy);

        //                var endPoint = CheckTransactionEndPoint.Replace("{MerchantAccountNumber}", AccountNumber);
        //                endPoint = endPoint.Replace("{TransactionId}", trans.TransactionId);

        //                var client = new RestClient(Hostname)
        //                {
        //                    Authenticator = new HttpBasicAuthenticator(ClientId, ClientSecret)
        //                };
        //                var request = new RestRequest(endPoint, Method.GET)
        //                {
        //                    RequestFormat = DataFormat.Json
        //                };

        //                var res = client.Execute(request);
        //                if (res.StatusCode == HttpStatusCode.OK)
        //                {
        //                    var result = JsonConvert.DeserializeObject<MomoResponse>(res.Content);
        //                    if (result.ResponseCode == "0000")
        //                    {
        //                        if (result.Data.First().TransactionStatus == "Settled")
        //                        {
        //                            trans.Status = SubscriptionPaymentStatus.Succeeded;

        //                            user.HasValidSubscription = true;
        //                            user.SubscriptionStartDate = DateTime.Now;
        //                            user.SubscriptionEndDate = user.SubscriptionStartDate.Value.AddMonths(trans.Subscription.SubscriptionPlan.Duration);
        //                            var subEndDate = user.SubscriptionEndDate.Value.ToShortDateString();
        //                            var msg = new Message
        //                            {
        //                                Text =
        //                                    $"Hello {user.Name}, Your subscription payment has been recieved and confirmed. Amount: GHS {trans.Amount}, Plan: GHS {trans.Subscription.SubscriptionPlan.Name}, Subscription End Date: {subEndDate}",
        //                                Subject = "Successful Subscription Payment",
        //                                Recipient = user.PhoneNumber,
        //                                TimeStamp = DateTime.Now
        //                            };
        //                            db.Messages.Add(msg);
        //                            db.SaveChanges();
        //                        }
        //                        else if (result.Data.First().TransactionStatus == "Failed")
        //                        {
        //                            trans.Status = SubscriptionPaymentStatus.Failed;

        //                            var msg = new Message
        //                            {
        //                                Text =
        //                                    $"Hello {user.Name}, Your tsubscription payment was not authorized. Please ensure that you have sufficient funds in your wallet.",
        //                                Subject = "Unsuccessful Subscription Payment",
        //                                Recipient = user.PhoneNumber,
        //                                TimeStamp = DateTime.Now
        //                            };
        //                            db.Messages.Add(msg);
        //                            db.SaveChanges();
        //                        }
        //                    }
        //                }
        //                trans.Response = res.Content;
        //                db.SaveChanges();
        //                transaction.Commit();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WebHelpers.ProcessException(ex);
        //    }
        //}

        //public static string GetProviderName(string name)
        //{
        //    var res = "";
        //    if (name.ToLower().Contains("mtn")) res = "mtn-gh";
        //    if (name.ToLower().Contains("tigo")) res = "tigo-gh";
        //    if (name.ToLower().Contains("airtel")) res = "airtel-gh";
        //    if (name.ToLower().Contains("vodaphone")) res = "vodaphone-gh";

        //    return res;
        //}
    }

    public class MomoResponse
    {
        public string ResponseCode { get; set; }
        public List<MomoResponseData> Data { get; set; }
    }

    public class MomoResponse1
    {
        public string ResponseCode { get; set; }
        public MomoResponseData Data { get; set; }
    }

    public class MomoResponseData
    {

        public string ErrorCode { get; set; }
        public string TransactionId { get; set; }
        public string ClientReference { get; set; }
        public double Amount { get; set; }
        public double Charges { get; set; }
        public string TransactionStatus { get; set; }
    }
}