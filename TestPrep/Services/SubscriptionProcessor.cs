using Quartz;
using System;
using System.Linq;
using TestPrep.AxHelpers;
using TestPrep.Models;

namespace TestPrep.Services
{
    public class SubscriptionProcessor
    {
        public void CheckExpiry()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var date = DateTime.Now;
                    var users = db.Users.Where(x => x.HasValidSubscription && x.SubscriptionEndDate <= date);
                    foreach (var user in users)
                    {
                        user.HasValidSubscription = false;
                        user.Updated = date;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception) { }
        }
    }

    [DisallowConcurrentExecution]
    public class SubscriptionProcessService : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            new SubscriptionProcessor().CheckExpiry();
        }
    }
}