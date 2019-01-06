using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using TestPrep.Models;

namespace TestPrep.AxHelpers
{
    [HubName("processhub")]
    public class ProcessHub : Hub { }

    public class SignalNotice
    {
        public SignalEvent SignalEvent { get; set; } = SignalEvent.General;
        public string Message { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
    }

    public enum SignalEvent
    {
        General
    }

    public class NotificationHelpers
    {
        public static void OnSignalEvent(SignalEvent evnt, string msg, object data)
        {
            var notHub = GlobalHost.ConnectionManager.GetHubContext<ProcessHub>();
            notHub.Clients.All.notify(new SignalNotice
            {
                SignalEvent = evnt,
                Success = true,
                Message = msg,
                Data = data
            });
        }

        public static void AddNotification(string type, string text, AppDbContext db)
        {
            var not = new Notification
            {
                Date = DateTime.Now,
                Message = text,
                Opened = false,
                Type = type
            };
            db.Notifications.Add(not);
            db.SaveChanges();
            var processHub = GlobalHost.ConnectionManager.GetHubContext<ProcessHub>();
            processHub.Clients.All.notify(new SignalNotice
            {
                Message = "New Notification",
                Success = true,
                Data = text
            });
        }

        public static void SendPushNotification(AppDbContext db, string userId, string message="")
        {
            //push notification
            var not = new UserPushNotification
            {
                Message = message,
                UserId = userId
            };
            db.UserPushNotifications.Add(not);
            
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            if (request != null)
            {
                request.KeepAlive = true;
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("authorization", "Basic ZDM1ZThhMzAtNThhNC00MDFmLWFkYzktNmI5YzAzMzcxNjEw");

                //get user 
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                if (user == null) return;

                var byteArray = Encoding.UTF8.GetBytes("{"
                                                       + "\"app_id\": \"e360b2bb-517b-4aae-b8f0-8643792c92e7\","
                                                       + "\"contents\": {\"en\": \"" + message + "\"},"
                                                       + "\"filters\": [{\"field\": \"tag\", \"key\": \"USERNAME\", \"relation\": \"=\", \"value\": \"" + user.UserName + "\"}]}");

                string responseContent = null;

                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    db.SaveChanges();
                    if (response != null)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            responseContent = reader.ReadToEnd();
                            var not1 = db.UserPushNotifications.First(x => x.Id == not.Id);
                            not1.IsSent = true;
                            not1.Response = responseContent;
                            db.SaveChanges();
                        }
                    }
                
                }

            }
            db.SaveChanges();
        }

        public static void ResendPushNotification(AppDbContext db, long notId)
        {
            var not = db.UserPushNotifications.Where(x => x.Id == notId).Include(x=> x.User).First();

            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            if (request != null)
            {
                request.KeepAlive = true;
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("authorization", "Basic ZDM1ZThhMzAtNThhNC00MDFmLWFkYzktNmI5YzAzMzcxNjEw");

                var byteArray = Encoding.UTF8.GetBytes("{"
                                                       + "\"app_id\": \"e360b2bb-517b-4aae-b8f0-8643792c92e7\","
                                                       + "\"contents\": {\"en\": \"" + not.Message + "\"},"
                                                       + "\"filters\": [{\"field\": \"tag\", \"key\": \"USERNAME\", \"relation\": \"=\", \"value\": \"" + not.User.UserName + "\"}]}");

                string responseContent = null;

                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    db.SaveChanges();
                    if (response != null)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            responseContent = reader.ReadToEnd();
                            not.IsSent = true;
                            not.Response = responseContent;
                            db.SaveChanges();
                        }
                    }

                }

            }
            db.SaveChanges();
        }
    }


}