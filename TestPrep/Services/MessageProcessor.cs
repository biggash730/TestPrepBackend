using System;
using System.Linq;
using Quartz;
using TestPrep.AxHelpers;
using TestPrep.Models;

namespace TestPrep.Services
{
    public class MessageProcessor
    {
        public void Send()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var unsentMsgs = db.Messages.Where(x => (x.Status == MessageStatus.Pending || x.Status == MessageStatus.Failed)).ToList();

                    foreach (var msg in unsentMsgs)
                    {
                        MessageHelpers.SendSms(msg.Id);
                    }
                }
            }
            catch (Exception) { }
        }

        public void ResendPushNotifications()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var unsentMsgs = db.UserPushNotifications.Where(x => (!x.IsSent)).ToList();

                    foreach (var msg in unsentMsgs)
                    {
                        NotificationHelpers.ResendPushNotification(new AppDbContext(), msg.Id);
                    }
                }
            }
            catch (Exception) { }
        }


    }
    [DisallowConcurrentExecution]
    public class MessageProcessService : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            new MessageProcessor().Send();
            new MessageProcessor().ResendPushNotifications();
        }
    }
}