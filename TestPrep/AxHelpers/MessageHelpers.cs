using System;
using System.Linq;
using System.Net;
using System.Text;
using hubtelapi_dotnet_v1.Hubtel;
using Newtonsoft.Json;
using RestSharp;
using TestPrep.Models;
using Message = TestPrep.Models.Message;

namespace TestPrep.AxHelpers
{
    public class MessageHelpers
    {
        public static void SendSms(long id)
        {
            using (var db = new AppDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var msg = db.Messages.First(x => x.Id == id);
                        var numbers = msg.Recipient;
                        numbers = numbers.Replace("-", "");
                        numbers = numbers.Replace(" ", "");
                        //strip the country code form it
                        if (numbers.StartsWith("00233"))
                        {
                            numbers = "233" + numbers.Substring(5);
                        }
                        if (numbers.StartsWith("+233")) numbers = numbers.Replace("+233", "233");
                        if (numbers.Length <= 10)
                        {
                            if (numbers.StartsWith("02"))
                            {
                                numbers = "2332" + numbers.Substring(2);
                            }
                            else if (numbers.StartsWith("05"))
                            {
                                numbers = "2335" + numbers.Substring(2);
                            }
                        }

                        var infobipMessage = new InfobipHelpers.InfobipSmsMessage
                        {
                            @from = "Test Prep",
                            text = msg.Text,
                            to = numbers
                        };
                        //send the sms messages
                        var client = new RestClient("https://api.infobip.com/sms/1/text/single");
                        var req = new RestRequest(Method.POST)
                        {
                            RequestFormat = DataFormat.Json
                        };
                        req.AddHeader("Accept", "application/json");
                        req.AddHeader("Content-Type", "application/json");
                        req.AddHeader("Authorization", "Basic Ymx1bWE6aW5mb2JpcFBAc3Mx");

                        var rq = JsonConvert.SerializeObject(infobipMessage);
                        req.AddParameter("application/json", rq, ParameterType.RequestBody);

                        var res = client.Execute(req);

                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            msg.Status = MessageStatus.Sent;
                            msg.Response = JsonConvert.SerializeObject(res);
                        }
                        else
                        {
                            msg.Status = MessageStatus.Failed;
                        }
                        msg.TimeStamp = DateTime.Now;
                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        WebHelpers.ProcessException(e);
                    }
                }
            }
        }

        public static void AddMessage(string subject, string text, User user, AppDbContext db)
        {
            var msg = new Message
            {
                Text = text,
                Subject = subject,
                Recipient = user.PhoneNumber,
                TimeStamp = DateTime.Now
            };
            db.Messages.Add(msg);
            db.SaveChanges();
        }
        public static string GenerateRandomString(int length)
        {
            var stringBuilder = new StringBuilder(length);
            var chArray = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            var random = new Random((int)DateTime.Now.Ticks);
            for (var index = 0; index < length; ++index)
                stringBuilder.Append(chArray[random.Next(chArray.Length)]);
            return stringBuilder.ToString().ToUpper();
        }
        public static string GenerateRandomNumber(int length)
        {
            var stringBuilder = new StringBuilder(length);
            var chArray = "0123456789".ToCharArray();
            var random = new Random((int)DateTime.Now.Ticks);
            for (var index = 0; index < length; ++index)
                stringBuilder.Append(chArray[random.Next(chArray.Length)]);
            return stringBuilder.ToString();
        }
    }
}