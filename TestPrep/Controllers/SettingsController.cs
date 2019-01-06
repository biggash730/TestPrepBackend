using System;
using System.Linq;
using System.Web.Http;
using TestPrep.AxHelpers;
using TestPrep.Extensions;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    public class ProfileController : BaseApi<Profile>
    {
        public override ResultObj Get()
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                var data = Repository.Get();
                results = WebHelpers.BuildResponse(data, "Records Loaded", true, data.Count);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }
            return results;
        }

        public override ResultObj Post(Profile record)
        {
            ResultObj results;
            try
            {
                var user = User.Identity.AsAppUser().Result;
                Repository.Insert(SetAudit(record, true));
                results = WebHelpers.BuildResponse(record, "New Profile Saved Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }
    }
}
