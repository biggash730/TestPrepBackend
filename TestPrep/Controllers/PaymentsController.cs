using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using TestPrep.AxHelpers;
using TestPrep.DataAccess.Filters;
using TestPrep.DataAccess.Repositories;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [RoutePrefix("api/payments")]
    public class PaymentsController : BaseApi<SubscriptionPayment>
    {
        [HttpPost]
        [Route("query")]
        public ResultObj Query(SubscriptionPaymentsFilter filter)
        {
            ResultObj results;
            try
            {
                var userId = User.Identity.GetUserId();
                filter.UserId = userId;
                var repo = new BaseRepository<SubscriptionPayment>();
                var raw = repo.Query(filter);
                var data = raw.OrderByDescending(x => x.Date).Skip(filter.Pager.Skip())
                    .Take(filter.Pager.Size).ToList();
                var res = data.Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Amount,
                        x.Response,
                        x.Date,
                        x.Reference,
                        x.Number,
                        x.Network,
                        x.Token,
                        x.TransactionId,
                        Status = x.Status.ToString(),
                        Plan = x.Subscription.SubscriptionPlan.Name,
                        x.Subscription.SubscriptionPlan.Duration
                    }).ToList();
                results = WebHelpers.BuildResponse(res, "Records Loaded", true, raw.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpPost]
        [Route("adminquery")]
        public ResultObj AdminQuery(SubscriptionPaymentsFilter filter)
        {
            ResultObj results;
            try
            {
                var repo = new BaseRepository<SubscriptionPayment>();
                var raw = repo.Query(filter);
                var data = raw.OrderByDescending(x => x.Date).Skip(filter.Pager.Skip())
                    .Take(filter.Pager.Size).ToList();
                var res = data.Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Amount,
                        x.Response,
                        x.Date,
                        x.Reference,
                        x.Number,
                        x.Network,
                        x.Token,
                        x.TransactionId,
                        Status = x.Status.ToString(),
                        Plan = x.Subscription.SubscriptionPlan.Name,
                        x.Subscription.SubscriptionPlan.Duration
                    }).ToList();
                results = WebHelpers.BuildResponse(res, "Records Loaded", true, raw.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Get()
        {
            ResultObj results;
            try
            {
                var db = new AppDbContext();
                var userId = User.Identity.GetUserId();
                var data = db.subscriptionPayments.Where(x => x.Subscription.UserId == userId).OrderByDescending(x => x.Date).ToList().Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Amount,
                        x.Response,
                        x.Date,
                        x.Reference,
                        x.Number,
                        x.Network,
                        x.Token,
                        x.TransactionId,
                        Status = x.Status.ToString(),
                        Plan = x.Subscription.SubscriptionPlan.Name,
                        x.Subscription.SubscriptionPlan.Duration
                    }).ToList();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Get(long id)
        {
            ResultObj results;
            try
            {
                var db = new AppDbContext();
                var data = db.subscriptionPayments.Where(x => x.Id == id).Include(x=> x.Subscription.SubscriptionPlan).OrderByDescending(x => x.Date).ToList().Select(x =>
                      new
                      {
                          x.CreatedAt,
                          x.CreatedBy,
                          x.ModifiedAt,
                          x.ModifiedBy,
                          x.Id,
                          x.Name,
                          x.Amount,
                          x.Response,
                          x.Date,
                          x.Reference,
                          x.Number,
                          x.Network,
                          x.Token,
                          x.TransactionId,
                          Status = x.Status.ToString(),
                          Plan = x.Subscription.SubscriptionPlan.Name,
                          x.Subscription.SubscriptionPlan.Duration
                      }).FirstOrDefault();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        public override ResultObj Post(SubscriptionPayment model)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                var rec = new SubscriptionPayment
                {
                    Amount = model.Amount,
                    Network = model.Network,
                    Number = model.Number,
                    SubscriptionId = model.SubscriptionId,
                    CreatedBy = username,
                    ModifiedBy = username,
                    ModifiedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                db.subscriptionPayments.Add(rec);
                

                var sub = db.Subscriptions.First(x => x.Id == model.SubscriptionId);
                sub.Status = SubscriptionStatus.Processing;

                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Saved Successfully", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Put(SubscriptionPayment rec)
        {
            ResultObj results;
            try
            {                
                results = WebHelpers.BuildResponse(null, "Not Implemented", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Delete(long id)
        {
            ResultObj results;
            try
            {
                results = WebHelpers.BuildResponse(null, "Not Implemented", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
    }
}
