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
    [RoutePrefix("api/kinds")]
    public class KindsController : BaseApi<Kind>
    {
        [HttpPost]
        [Route("query")]
        public ResultObj Query(KindsFilter filter)
        {
            ResultObj results;
            try
            {
                var repo = new BaseRepository<Kind>();
                var raw = repo.Query(filter);
                var data = raw.OrderBy(x => x.Name).ToList();
                if (filter.Pager.Page > 0)
                {
                    data = data.Skip(filter.Pager.Skip())
                        .Take(filter.Pager.Size).ToList();
                }
                var res = data.Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Notes,
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
                var data = db.Kinds.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList().Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.Name,
                        x.Notes
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
                var data = db.Kinds.Where(x => x.Id == id).OrderBy(x => x.Name).ToList().Select(x =>
                      new
                      {
                          x.CreatedAt,
                          x.CreatedBy,
                          x.ModifiedAt,
                          x.ModifiedBy,
                          x.Id,
                          x.Name,
                          x.Notes
                      }).First();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        public override ResultObj Post(Kind rec)
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
                db.Kinds.Add(rec);
                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Saved Successfully", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Put(Kind rec)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                var cat = db.Kinds.FirstOrDefault(x => x.Id == rec.Id);
                if (cat == null) throw new Exception("Please check the Id");
                cat.ModifiedBy = username;
                cat.ModifiedAt = DateTime.Now;
                cat.Name = rec.Name;
                cat.Notes = rec.Notes;
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
                var ext = db.Kinds.FirstOrDefault(x => x.Id == id);
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
