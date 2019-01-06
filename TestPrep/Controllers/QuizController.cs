using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.AspNet.Identity;
using TestPrep.AxHelpers;
using TestPrep.Classes.Repos;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [Authorize]
    [RoutePrefix("api/quiz")]
    [EnableCors("*", "*", "*")]
    public class QuizController : ApiController
    {
        [HttpPost]
        [Route("getquestions")]
        public ResultObj GetQuestions(GetQuestionsModel obj)
        {
            return new QuizRepo().GetQuestions(obj, User.Identity.GetUserId());
        }

        [HttpPost]
        [Route("markquestions")]
        public ResultObj MarkQuestions(MarkQuestionsModel model)
        {
            return new QuizRepo().MarkQuestions(model, User.Identity.GetUserId());
        }

        [HttpPost]
        [Route("getresultx")]
        public ResultObj GetResultx(Filter filter)
        {
            return new QuizRepo().GetResults(filter, User.Identity.GetUserId());
        }

        [HttpGet]
        [Route("getresult")]
        public ResultObj GetResult(long id)
        {
            return new QuizRepo().GetResult(id, User.Identity.GetUserId());
        }

        [HttpPost]
        [Route("getresults")]
        public ResultObj GetResults(Filter filter)
        {
            return new QuizRepo().GetResults(filter, User.Identity.GetUserId());
        }

        [HttpGet]
        [Route("GetPercentageCorrectByCategories")]
        public ResultObj GetPercentageCorrectByCategories()
        {
            return new QuizRepo().GetPercentageCorrectByCategories(User.Identity.GetUserId());
        }
        [HttpGet]
        [Route("GetTestCompositionByCategories")]
        public ResultObj GetTestCompositionByCategories()
        {
            return new QuizRepo().GetTestCompositionByCategories(User.Identity.GetUserId());
        }


        [HttpGet]
        [Route("GetLeastperformedCategories")]
        public ResultObj GetLeastperformedCategories()
        {
            return new QuizRepo().GetLeastperformedCategories(User.Identity.GetUserId());
        }
    }
}
