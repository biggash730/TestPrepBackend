using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using TestPrep.AxHelpers;
using TestPrep.DataAccess.Filters;
using TestPrep.Extensions;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/public")]
    public class PublicController : ApiController
    {

        [HttpPost]
        [AllowAnonymous]
        [Route("receivemobilemoneycallback")]
        public async Task<ResultObj> ReceiveMobileMoneyCallBack(MomoResponse1 response)
        {
            try
            {
                using (var db = new AppDbContext())
                {

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        var trans =
                            db.subscriptionPayments.Where(x => x.TransactionId == response.Data.TransactionId).Include(x => x.Subscription.SubscriptionPlan).FirstOrDefault();
                        if (trans == null) throw new Exception("Wrong transaction Id");
                        var user = db.Users.First(x => x.Id == trans.Subscription.UserId);
                        var sub = db.Subscriptions.First(x => x.Id == trans.SubscriptionId);
                        if (response.ResponseCode == "0000")
                        {
                            trans.Status = SubscriptionPaymentStatus.Succeeded;
                            var date = DateTime.Now;
                            sub.Status = SubscriptionStatus.Paid;
                            if (user.HasValidSubscription)
                            {
                                user.SubscriptionEndDate = user.SubscriptionEndDate.Value.AddDays(trans.Subscription.SubscriptionPlan.Duration);
                            }
                            else
                            {
                                user.HasValidSubscription = true;
                                user.SubscriptionStartDate = date;
                                user.SubscriptionEndDate = date.AddDays(trans.Subscription.SubscriptionPlan.Duration);
                            }
                            db.SaveChanges();


                            var msg = new Message
                            {
                                Text =
                                        $"Hello {user.Name}, Your subscription payment request has been recieved and confirmed. Amount:{trans.Amount}, Payment Date: {trans.Date.ToShortDateString()}, Plan: {trans.Subscription.SubscriptionPlan.Name}",
                                Subject = "Successful Subscription Payment",
                                Recipient = user.PhoneNumber,
                                TimeStamp = DateTime.Now
                            };
                            db.Messages.Add(msg);

                            db.SaveChanges();

                        }
                        else
                        {
                            trans.Status = SubscriptionPaymentStatus.Failed;
                            sub.Status = SubscriptionStatus.PaymentFailed;
                            var msg = new Message
                            {
                                Text =
                                        $"Hello {user.Name}, Your subscription payment request was not authorized. Please ensure that you have sufficient funds in your wallet.",
                                Subject = "Failed Subscription Payment",
                                Recipient = user.PhoneNumber,
                                TimeStamp = DateTime.Now
                            };
                            db.Messages.Add(msg);

                            db.SaveChanges();
                        }
                        await db.SaveChangesAsync();
                        transaction.Commit();
                    }
                    return WebHelpers.BuildResponse(null, "Successful.", true, 1);
                }
            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

    }
}
