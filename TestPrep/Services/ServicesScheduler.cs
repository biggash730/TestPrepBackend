using Quartz;
using Quartz.Impl;

namespace TestPrep.Services
{
    public class ServicesScheduler
    {
        public static void Start()
        {
            var scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            var subscriptionService = JobBuilder.Create<SubscriptionProcessService>().Build();
            var messageService = JobBuilder.Create<MessageProcessService>().Build();
            var hubtelService = JobBuilder.Create<HubtelProcessService>().Build();
            var msgTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())
                    .Build();
            var hubtelTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())
                    .Build();
            var subscriptionTrigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())
                    .Build();

            scheduler.ScheduleJob(messageService, msgTrigger);
            scheduler.ScheduleJob(subscriptionService, subscriptionTrigger);
            scheduler.ScheduleJob(hubtelService, hubtelTrigger);

        }
    }
}