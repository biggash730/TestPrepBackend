using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Web.Http;
using TestPrep.AxHelpers;
using TestPrep.DataAccess.Filters;
using TestPrep.DataAccess.Repositories;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [RoutePrefix("api/plans")]
    public class PlansController : BaseApi<SubscriptionPlan>
    {
        [HttpPost]
        [Route("query")]
        public ResultObj Query(SubscriptionPlansFilter filter)
        {
            ResultObj results;
            try
            {
                var repo = new BaseRepository<SubscriptionPlan>();
                var raw = repo.Query(filter);
                var data = raw.OrderBy(x => x.Amount).Skip(filter.Pager.Skip())
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
                        x.Duration
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
        [Route("getpaidplans")]
        public ResultObj GetPaidPlans()
        {
            ResultObj results;
            try
            {
                var db = new AppDbContext();
                var data = db.SubscriptionPlans.Where(x => !x.IsDeleted && x.Amount > 0).OrderBy(x => x.Amount).ToList().Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Amount,
                        x.Duration
                    }).ToList();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count());
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
                var data = db.SubscriptionPlans.Where(x => x.Id > 0).OrderBy(x => x.Amount).ToList().Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Amount,
                        x.Duration
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
                var data = db.SubscriptionPlans.Where(x => x.Id == id).OrderBy(x => x.Amount).ToList().Select(x =>
                      new
                      {
                          x.CreatedAt,
                          x.CreatedBy,
                          x.ModifiedAt,
                          x.ModifiedBy,
                          x.Id,
                          x.Name,
                          x.Amount,
                          x.Duration
                      }).First();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        public override ResultObj Post(SubscriptionPlan rec)
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
                db.SubscriptionPlans.Add(rec);
                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Saved Successfully", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Put(SubscriptionPlan rec)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                var cat = db.SubscriptionPlans.FirstOrDefault(x => x.Id == rec.Id);
                if (cat == null) throw new Exception("Please check the Id");
                cat.ModifiedBy = username;
                cat.ModifiedAt = DateTime.Now;
                cat.Name = rec.Name;
                cat.Amount = rec.Amount;
                cat.Duration = rec.Duration;
                db.SaveChanges();
                results = WebHelpers.BuildResponse(rec.Id, "Updated Successfully", true, 1);
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
                var ext = db.SubscriptionPlans.FirstOrDefault(x => x.Id == id);
                if (ext == null) throw new Exception("Please check the Id");
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
