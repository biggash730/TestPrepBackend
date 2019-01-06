using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using TestPrep.AxHelpers;
using TestPrep.Extensions;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [Authorize]
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        [HttpGet]
        [Route("getstats")]
        public ResultObj GetStats()
        {
            ResultObj results;
            try
            {
                var res = new List<TransactionsSummary>();
                var user = User.Identity.AsAppUser().Result;
                using (var db = new AppDbContext())
                {
                    var year = DateTime.Now.Year;
                    var nhilOutTrans = 0.0;
                    var nhilInTrans = 0.0;
                    var flatOutTrans = 0.0;
                    //vat + nhil
                    var nhilTrans = db.VatNhilTransactions.Where(x => x.Date.Year == year && x.UserId == user.Id).ToList();
                    nhilOutTrans = nhilTrans.Where(x => x.Type == TransactionType.Output).ToList().Sum(x => x.Tax);
                    nhilInTrans = nhilTrans.Where(x => x.Type == TransactionType.Input).ToList().Sum(x => x.Tax);
                    res.Add(new TransactionsSummary
                    {
                        VatType = "VAT + NHIL 17.5%",
                        InputTransactions = nhilInTrans,
                        OutputTransactions = nhilOutTrans,
                        TotalAmount = nhilOutTrans - nhilInTrans
                    });

                    //vat flat rate
                    var flatTrans = db.VatFlatTransactions.Where(x => x.Date.Year == year && x.UserId == user.Id).ToList();
                    flatOutTrans = flatTrans.Sum(x => x.Tax);
                    res.Add(new TransactionsSummary
                    {
                        VatType = "VAT FLAT RATE 3%",
                        InputTransactions = 0,
                        OutputTransactions = flatOutTrans,
                        TotalAmount = flatOutTrans
                    });
                }
                
                results = WebHelpers.BuildResponse(res, "Loaded Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        
    }


    public class TransactionsSummary
    {
        public string VatType { get; set; } = "";
        public double OutputTransactions { get; set; } = 0.0;
        public double InputTransactions { get; set; } = 0.0;
        public double TotalAmount { get; set; } = 0.0;
    }
}
