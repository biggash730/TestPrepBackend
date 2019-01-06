using System;
using System.Linq;
using Quartz;
using TestPrep.AxHelpers;
using TestPrep.Models;

namespace TestPrep.Services
{
    public class HubtelProcessor
    {
        private int _batchSize = 50;

        public void ProcessTransactions()
        {
            try
            {
                var db = new AppDbContext();
                var trans =
                    db.subscriptionPayments.Where(x => x.Status == SubscriptionPaymentStatus.Pending)
                        .OrderBy(x => x.Date)
                        .Take(_batchSize)
                        .ToList();

                foreach (var t in trans)
                {
                    HubtelHelpers.ReceiveMobileMoney(t.Id);
                }
            }
            catch (Exception ex)
            {
                WebHelpers.ProcessException(ex);
            }
        }
    }

    [DisallowConcurrentExecution]
    public class HubtelProcessService : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            new HubtelProcessor().ProcessTransactions();
        }
    }


}