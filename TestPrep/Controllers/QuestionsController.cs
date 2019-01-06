using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using TestPrep.AxHelpers;
using TestPrep.DataAccess.Filters;
using TestPrep.DataAccess.Repositories;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [RoutePrefix("api/questions")]
    public class QuestionsController : BaseApi<Question>
    {
        [HttpPost]
        [Route("query")]
        public ResultObj Query(QuestionsFilter filter)
        {
            ResultObj results;
            try
            {
                var repo = new BaseRepository<Question>();
                var raw = repo.Query(filter);
                var data = raw.OrderBy(x => x.Category.Name).ThenBy(x => x.Batch).ToList();
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
                        x.QuestionText,
                        x.CategoryId,
                        Category = x.Category.Name,
                        x.Category.TypeId,
                        Type = x.Category.Type.Name,
                        x.Category.Type.KindId,
                        Kind = x.Category.Type.Kind.Name,
                        x.Answer,
                        x.Batch,
                        x.Option1,
                        x.Option2,
                        x.Option3,
                        x.Option4,
                        x.Option5,
                        x.Reason
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
                var data = db.Questions.Where(x => x.Id > 0).OrderBy(x => x.Category.Name).ThenBy(x=> x.Batch).ToList().Select(x =>
                    new
                    {
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Id,
                        x.QuestionText,
                        x.CategoryId,
                        Category = x.Category.Name,
                        x.Category.TypeId,
                        Type = x.Category.Type.Name,
                        x.Category.Type.KindId,
                        Kind = x.Category.Type.Kind.Name,
                        x.Answer,
                        x.Batch,
                        x.Option1,
                        x.Option2,
                        x.Option3,
                        x.Option4,
                        x.Option5,
                        x.Reason
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
                var data = db.Questions.Where(x => x.Id == id).OrderBy(x => x.Batch).ToList().Select(x =>
                      new
                      {
                          x.CreatedAt,
                          x.CreatedBy,
                          x.ModifiedAt,
                          x.ModifiedBy,
                          x.Id,
                          x.QuestionText,
                          x.CategoryId,
                          Category = x.Category.Name,
                          x.Category.TypeId,
                          Type = x.Category.Type.Name,
                          x.Category.Type.KindId,
                          Kind = x.Category.Type.Kind.Name,
                          x.Answer,
                          x.Batch,
                          x.Option1,
                          x.Option2,
                          x.Option3,
                          x.Option4,
                          x.Option5,
                          x.Reason
                      }).FirstOrDefault();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }
        public override ResultObj Post(Question rec)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                rec.Batch = "Batch_" + MessageHelpers.GenerateRandomNumber(4);
                rec.CreatedBy = username;
                rec.ModifiedBy = username;
                rec.ModifiedAt = DateTime.Now;
                rec.CreatedAt = DateTime.Now;
                db.Questions.Add(rec);
                db.SaveChanges();
                results = WebHelpers.BuildResponse(null, "Saved Successfully", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Put(Question rec)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                var db = new AppDbContext();
                var or = db.Questions.FirstOrDefault(x => x.Id == rec.Id);
                if (or == null) throw new Exception("Please check the Id");
                or.ModifiedBy = username;
                or.ModifiedAt = DateTime.Now;
                or.QuestionText = rec.QuestionText;
                or.CategoryId = rec.CategoryId;
                or.Answer = rec.Answer;
                or.Option1 = rec.Option1;
                or.Option2 = rec.Option2;
                or.Option3 = rec.Option3;
                or.Option4 = rec.Option4;
                or.Option5 = rec.Option5;
                or.Reason = rec.Reason;
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
                var ext = db.Questions.FirstOrDefault(x => x.Id == id);
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
        [HttpPost]
        [Route("saveupload")]
        public ResultObj SaveUpload(List<Question> questions)
        {
            ResultObj results;
            try
            {
                var username = User.Identity.GetUserName();
                using (var db = new AppDbContext())
                {
                    if (!questions.Any()) throw new Exception("There are no questons to upload");

                    var batch = "Batch_" + MessageHelpers.GenerateRandomNumber(4);


                    foreach (var q in questions)
                    {
                        var cat = db.Categories.FirstOrDefault(x => x.Name.ToLower() == q.CategoryName.ToLower());
                        if (cat == null) continue;
                        var exist =
                            db.Questions.FirstOrDefault(
                                x =>
                                    x.CategoryId == cat.Id && x.QuestionText == q.QuestionText &&
                                    x.Answer == q.Answer && !x.IsDeleted);
                        if (exist == null)
                        {
                            var nq = new Question
                            {
                                QuestionText = q.QuestionText.Trim(),
                                CategoryId = cat.Id,
                                Batch = batch,
                                Answer = q.Answer.Trim(),
                                Option1 = q.Option1.Trim(),
                                Option2 = q.Option2.Trim(),
                                Option3 = q.Option3.Trim(),
                                Option4 = q.Option4.Trim(),
                                Option5 = q.Option5.Trim(),
                                ModifiedAt = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                CreatedBy = username,
                                ModifiedBy = username,
                                Reason = q.Reason
                            };
                            db.Questions.Add(nq);
                        }
                        else
                        {
                            exist.ModifiedBy = username;
                            exist.ModifiedAt = DateTime.Now;
                            //exist.QuestionText = q.QuestionText;
                            //exist.CategoryId = q.CategoryId;
                            //exist.Answer = q.Answer;
                            exist.Batch = batch;
                            exist.Option1 = q.Option1;
                            exist.Option2 = q.Option2;
                            exist.Option3 = q.Option3;
                            exist.Option4 = q.Option4;
                            exist.Option5 = q.Option5;
                            exist.Reason = q.Reason;
                        }

                    }
                    db.SaveChanges();

                    results = WebHelpers.BuildResponse(null, "Uploaded successfully", true, 1);
                }
            }
            catch (Exception e)
            {
                results = WebHelpers.ProcessException(e);
            }
            return results;
        }
    }
}
