using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Web;
using TestPrep.Models;
using TestPrep.AxHelpers;

namespace TestPrep.Classes.Repos
{
    public class QuizRepo
    {
        public ResultObj GetQuestions(GetQuestionsModel obj, string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    //check if the user has a valid subscription
                    var us = db.Users.First(x => x.Id == userId);
                    if (!us.HasValidSubscription) throw new Exception("You do not have a valid subscription, please go to the portal to subscribe to a plan.");

                    if (obj.CategoryIds.Count <= 0) throw new Exception("Select at least 1 category");
                    var data = db.Questions.Include(x => x.Category).Where(x => !x.IsDeleted && x.IsActive && obj.CategoryIds.Contains(x.CategoryId)).OrderBy(x => Guid.NewGuid()).Take(obj.NumberOfQuestions).ToList();

                    var res =
                        data.Select(
                            x =>
                                new {
                                    x.QuestionText,
                                    x.Id,
                                    Category = x.Category.Name,
                                    x.CategoryId,
                                    x.Option1,
                                    x.Option2,
                                    x.Option3,
                                    x.Option4,
                                    x.Option5
                                }).ToList();

                    return res.Any() ? WebHelpers.BuildResponse(res, "Loaded successfully", true, res.Count()) : WebHelpers.BuildResponse(null, "No Questions Found", false, 0);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public ResultObj MarkQuestions(MarkQuestionsModel model, string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var ques = model.Questions;
                    var questionIds = ques.Select(x => x.Id).ToList();
                    var questions = db.Questions.Include(x => x.Category).Where(x => questionIds.Contains(x.Id)).ToList();
                    var catIds = ques.Select(x => x.CategoryId).Distinct().ToList();
                    var cats = db.Categories.Where(x => catIds.Contains(x.Id)).Select(x => x.Name).ToList();

                    var revQues = new List<ReviewQuestion>();
                    var resModel = new ResultModel();
                    var correctQ = new List<long>();
                    var wrongQ = new List<long>();

                    var res = new Result
                    {
                        CategoryIds = string.Join(",", catIds),
                        //Categories = string.Join(",",cats),
                        TotalQuestions = ques.Count(),
                        Date = DateTime.Now,
                        UserId = userId,
                        TotalCorrect = 0,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = userId,
                        ModifiedBy = userId,
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        QuestionsList = string.Join(",", questionIds),
                        TimeTaken = model.TimeTaken,
                        Duration = model.Duration,
                    };
                    var count = 1;
                    foreach (var x in ques)
                    {
                        var rq = new ReviewQuestion();
                        var q = questions.First(a => a.Id == x.Id);
                        rq.QuestionText = q.QuestionText;
                        rq.Answer = q.Answer;
                        rq.Option1 = q.Option1;
                        rq.Option2 = q.Option2;
                        rq.Option3 = q.Option3;
                        rq.Option4 = q.Option4;
                        rq.SelectedOption = x.Answer;
                        rq.QuestionNumber = count;
                        rq.Category = q.Category.Name;
                        if (q.Answer == x.Answer)
                        {
                            res.TotalCorrect++;
                            correctQ.Add(q.Id);
                        }
                        else
                        {
                            wrongQ.Add(q.Id);
                        }
                        revQues.Add(rq);
                        count++;
                    }
                    res.CorrectQuestions = string.Join(",", correctQ);
                    res.WrongQuestions = string.Join(",", wrongQ);
                    db.Results.Add(res);
                    db.SaveChanges();

                    resModel.ReviewQuestions = revQues;
                    resModel.Categories = string.Join(",", cats);
                    resModel.Date = res.Date;
                    resModel.TotalCorrect = res.TotalCorrect;
                    resModel.TotalQuestions = res.TotalQuestions;
                    return WebHelpers.BuildResponse(resModel, "Loaded successfully", true, 0);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public ResultObj GetResults(Filter filter, string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var data = db.Results.Where(x => !x.IsDeleted && x.IsActive && x.UserId == userId && x.CreatedBy == userId).OrderByDescending(x => x.Date).ToList();
                    var cats = db.Categories.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    var total = data.Count();
                    if (filter.Page > 0)
                    {
                        data = data.OrderByDescending(x => x.Date).Take(filter.Size * filter.Page).ToList();
                    }
                    foreach (var res in data)
                    {
                        var catIds = res.CategoryIds.Split(',').Select(long.Parse).ToList();
                        var catNames = cats.Where(x => catIds.Contains(x.Id)).Select(x => x.Name).ToList();
                        res.Categories = string.Join(",", catNames);
                        res.User = null;

                    }
                    if(!data.Any()) return WebHelpers.BuildResponse(data, "No Data Found", false, 0);

                    var results = data.Select(x => new
                    {
                        x.UserId,
                        x.TimeTaken,
                        x.TotalCorrect,
                        x.TotalQuestions,
                        x.Percentage,
                        x.Categories,
                        x.Date,
                        x.Duration
                    }).ToList();

                    return WebHelpers.BuildResponse(results, "Loaded successfully", true, results.Count);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public ResultObj GetResult(long id, string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var data = db.Results.Where(x => !x.IsDeleted && x.IsActive && x.UserId == userId && x.CreatedBy == userId && x.Id == id).ToList();
                    var cats = db.Categories.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    
                    foreach (var res in data)
                    {
                        var catIds = res.CategoryIds.Split(',').Select(long.Parse).ToList();
                        var catNames = cats.Where(x => catIds.Contains(x.Id)).Select(x => x.Name).ToList();
                        res.Categories = string.Join(",", catNames);
                        res.User = null;
                    }
                    if (!data.Any()) return WebHelpers.BuildResponse(data, "No Data Found", false, 0);

                    var results = data.Select(x => new
                    {
                        x.UserId,
                        x.TimeTaken,
                        x.TotalCorrect,
                        x.TotalQuestions,
                        x.QuestionsList,
                        x.Categories,
                        x.CategoryIds,
                        x.CorrectQuestions,
                        Percentage = (x.TotalCorrect / x.TotalQuestions) * 100,
                        x.Date,
                        x.Duration
                    }).First();

                    return WebHelpers.BuildResponse(results, "Loaded successfully", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public ResultObj GetPercentageCorrectByCategories(string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var pcbt = new PieChatData
                    {
                        Data = new List<double>(),
                        Labels = new List<string>()
                    };
                    var correctQueIds = new List<long>();
                    var quesIds = new List<long>();
                    var results = db.Results.Where(x => !x.IsDeleted && x.IsActive && x.UserId == userId && x.CreatedBy == userId).ToList();
                    var cats = db.Categories.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    foreach (var r in results)
                    {
                        quesIds.AddRange(r.QuestionsList.Split(',').Select(long.Parse));
                        correctQueIds.AddRange(r.CorrectQuestions.Split(',').Select(long.Parse));
                    }

                    var questions = db.Questions.Where(x => quesIds.Contains(x.Id)).ToList();

                    foreach (var c in cats)
                    {
                        var catQuestions = questions.Where(x => x.CategoryId == c.Id).ToList();
                        var questionsCount = catQuestions.Count();
                        var numberCorrect = catQuestions.Where(x => correctQueIds.Contains(x.Id)).ToList().Count();
                        var ave = (double)(numberCorrect * 100) / questionsCount;
                        if (!(ave > 0)) continue;
                        pcbt.Data.Add((double.Parse(ave.ToString("#.##"))));
                        pcbt.Labels.Add(c.Name);
                    }
                    return WebHelpers.BuildResponse(pcbt, "Loaded successfully", true, 0);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public ResultObj GetTestCompositionByCategories(string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var pcbt = new PieChatData
                    {
                        Data = new List<double>(),
                        Labels = new List<string>()
                    };
                    var allCatIds = new List<long>();
                    var resultsCatIds = db.Results.Where(x => !x.IsDeleted && x.IsActive && x.UserId == userId && x.CreatedBy == userId).Select(x => x.CategoryIds).ToList();
                    foreach (var rc in resultsCatIds)
                    {
                        allCatIds.AddRange(rc.Split(',').Select(long.Parse));
                    }
                    var cats = db.Categories.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    foreach (var c in cats)
                    {
                        var cnt = allCatIds.Where(x => x == c.Id).ToList().Count();
                        if (cnt <= 0) continue;
                        pcbt.Data.Add(cnt);
                        pcbt.Labels.Add(c.Name);
                    }
                    return WebHelpers.BuildResponse(pcbt, "Loaded successfully", true, 0);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public ResultObj GetLeastperformedCategories(string userId)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var correctQueIds = new List<long>();
                    var quesIds = new List<long>();
                    var results = db.Results.Where(x => !x.IsDeleted && x.IsActive && x.UserId == userId && x.CreatedBy == userId).ToList();
                    var cats = db.Categories.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    foreach (var r in results)
                    {
                        quesIds.AddRange(r.QuestionsList.Split(',').Select(long.Parse));
                        correctQueIds.AddRange(r.CorrectQuestions.Split(',').Select(long.Parse));
                    }

                    var questions = db.Questions.Where(x => quesIds.Contains(x.Id)).ToList();

                    var pcbt = (from c in cats let catQuestions = questions.Where(x => x.CategoryId == c.Id).ToList() let questionsCount = catQuestions.Count() let numberCorrect = catQuestions.Where(x => correctQueIds.Contains(x.Id)).ToList().Count() let ave = (double)(numberCorrect * 100) / questionsCount select new CategoryData { Data = ((double.Parse(ave.ToString("#.##")))), Label = c.Name }).ToList();
                    pcbt = pcbt.Where(x => x.Data < 50).OrderBy(x => x.Data).Take(5).ToList();
                    return WebHelpers.BuildResponse(pcbt, "Loaded successfully", true, 0);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        public class PieChatData
        {
            public List<double> Data { get; set; }
            public List<string> Labels { get; set; }
        }

        public class CategoryData
        {
            public double Data { get; set; }
            public string Label { get; set; }
        }
    }
}