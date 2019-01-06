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
    [RoutePrefix("api/subscriptions")]
    public class SubscriptionsController : BaseApi<Subscription>
    {
        [HttpPost]
        [Route("adminquery")]
        public ResultObj AdminQuery(SubscriptionsFilter filter)
        {
            ResultObj results;
            try
            {
                var repo = new BaseRepository<Subscription>();
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
                        x.Date,
                        Status = x.Status.ToString(),
                        x.UserId,
                        User = x.User.Name,
                        Username = x.User.UserName,
                        Plan = x.SubscriptionPlan.Name,
                        x.SubscriptionPlan.Duration,
                        x.SubscriptionPlan.Amount
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
        [Route("query")]
        public ResultObj Query(SubscriptionsFilter filter)
        {
            ResultObj results;
            try
            {
                var userId = User.Identity.GetUserId();
                filter.UserId = userId;
                var repo = new BaseRepository<Subscription>();
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
                        x.Date,
                        Status = x.Status.ToString(),
                        x.UserId,
                        User = x.User.Name,
                        Username = x.User.UserName,
                        Plan = x.SubscriptionPlan.Name,
                        x.SubscriptionPlan.Duration,
                        x.SubscriptionPlan.Amount
                    }).ToList();
                results = WebHelpers.BuildResponse(res, "Records Loaded", true, raw.Count());
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        [HttpGet]
        [Route("add")]
        public ResultObj Add(long planId)
        {
            ResultObj results;
            try
            {
                var userId = User.Identity.GetUserId();
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                var plan = db.SubscriptionPlans.FirstOrDefault(x => x.Id == planId);
                if (plan == null) throw new Exception("Please check the selected plan");
                var rec = new Subscription
                {
                    SubscriptionPlanId = plan.Id,
                    UserId = userId,
                    Date = DateTime.Now,
                    Status = SubscriptionStatus.Pending,
                    CreatedBy = username,
                    ModifiedBy = username,
                    ModifiedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                db.Subscriptions.Add(rec);
                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Added Successfully, Please proceed to make payment.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        [HttpGet]
        [Route("getactive")]
        public ResultObj GetActive()
        {
            ResultObj results;
            try
            {
                var userId = User.Identity.GetUserId();
                var db = new AppDbContext();
                var date = DateTime.Now;
                var user = db.Users.First(x => x.Id == userId);
                if (!user.HasValidSubscription) throw new Exception("No active subscription");
                var res = new
                {
                    user.SubscriptionStartDate,
                    user.SubscriptionEndDate,
                    Duration = (user.SubscriptionEndDate.Value - user.SubscriptionStartDate.Value).TotalDays,
                    DaysToExpiry = (user.SubscriptionEndDate.Value - date).TotalDays
                };
                results = WebHelpers.BuildResponse(res, "Record Loaded", true, 1);
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
                var data = db.Subscriptions.Where(x => x.Id > 0).Include(x => x.SubscriptionPlan).OrderByDescending(x => x.Date).ToList().Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Date,
                        Status = x.Status.ToString(),
                        x.UserId,
                        User = x.User.Name,
                        Username = x.User.UserName,
                        Plan = x.SubscriptionPlan.Name,
                        x.SubscriptionPlan.Duration,
                        x.SubscriptionPlan.Amount
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
                var data = db.Subscriptions.Where(x => x.Id == id).Include(x=> x.SubscriptionPlan).OrderByDescending(x => x.Date).ToList().Select(x =>
                      new
                      {
                          x.CreatedAt,
                          x.CreatedBy,
                          x.ModifiedAt,
                          x.ModifiedBy,
                          x.Id,
                          x.Date,
                          Status = x.Status.ToString(),
                          x.UserId,
                          User = x.User.Name,
                          Username = x.User.UserName,
                          Plan = x.SubscriptionPlan.Name,
                          x.SubscriptionPlan.Duration,
                          x.SubscriptionPlan.Amount
                      }).First();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        public override ResultObj Post(Subscription rec)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                rec.CreatedBy = username;
                rec.ModifiedBy = username;
                rec.ModifiedAt = DateTime.Now;
                rec.CreatedAt = DateTime.Now;
                db.Subscriptions.Add(rec);
                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Saved Successfully", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        public override ResultObj Put(Subscription rec)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
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
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                var ext = db.Subscriptions.FirstOrDefault(x => x.Id == id);
                if (ext == null) throw new Exception("Please check the Id");
                if(ext.Status != SubscriptionStatus.Pending) throw new Exception("You can only delete pending subscriptions.");
                ext.IsDeleted = true;
                ext.ModifiedBy = username;
                ext.ModifiedAt = DateTime.Now;
                db.SaveChanges();
                results = WebHelpers.BuildResponse(id, "Deleted Successfully", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
    }
}
