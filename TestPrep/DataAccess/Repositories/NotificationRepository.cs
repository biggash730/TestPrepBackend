using System;
using System.IO;
using System.Net;
using System.Text;
using OneSignal.CSharp.SDK.Resources.Notifications;
using OneSignal.CSharp.SDK.Serializers;
using RestSharp;

namespace TestPrep.DataAccess.Repositories
{
    public class NotificationRepository
    {
        public string OneSignalApiKey = "ZDM1ZThhMzAtNThhNC00MDFmLWFkYzktNmI5YzAzMzcxNjEw";
        public string OneSignalAppId = "e360b2bb-517b-4aae-b8f0-8643792c92e7";

        public void SendToOneSignal(string message, string username=null)
        {
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers.Add("authorization", "Basic ZDM1ZThhMzAtNThhNC00MDFmLWFkYzktNmI5YzAzMzcxNjEw");

            var msg = message;
            var notif = "";
            if (username != null)
            {
                notif = "{"
                        + "\"app_id\": \"e360b2bb-517b-4aae-b8f0-8643792c92e7\","
                        + "\"contents\": {\"en\": \"" + msg + "\"},"
                        + "\"filters\": [{\"field\": \"tag\", \"key\": \"USERNAME\", \"relation\": \"=\", \"value\": \"" +
                        username + "\"}]}";
            }
            else
            {
                notif = "{"
                        + "\"app_id\": \"e360b2bb-517b-4aae-b8f0-8643792c92e7\","
                        + "\"contents\": {\"en\": \"" + msg + "\"}}";
            }
            var byteArray = Encoding.UTF8.GetBytes(notif);

            string responseContent = null;

            using (var writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    responseContent = reader.ReadToEnd();
                }
            }
        }

        public NotificationCreateResult SendToOneSignalX(string message, string username = null)
        {
            var options = new NotificationCreateOptions();
            var client = new RestClient("https://onesignal.com/api/v1");
            var restRequest = new RestRequest("notifications", Method.POST);

            restRequest.AddHeader("Authorization", "Basic ZDM1ZThhMzAtNThhNC00MDFmLWFkYzktNmI5YzAzMzcxNjEw");

            restRequest.RequestFormat = DataFormat.Json;
            restRequest.JsonSerializer = new NewtonsoftJsonSerializer();
            restRequest.AddBody(options);

            var restResponse = client.Execute<NotificationCreateResult>(restRequest);

            if (!(restResponse.StatusCode != HttpStatusCode.Created || restResponse.StatusCode != HttpStatusCode.OK))
            {
                if (restResponse.ErrorException != null)
                {
                    throw restResponse.ErrorException;
                }
                else if (restResponse.StatusCode != HttpStatusCode.OK && restResponse.Content != null)
                {
                    throw new Exception(restResponse.Content);
                }
            }

            return restResponse.Data;
        }
    }

    public class NotificationFilter : INotificationFilter
    {
        public string field { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string relation { get; set; }


    }
}