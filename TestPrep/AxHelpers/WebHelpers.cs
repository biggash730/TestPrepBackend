using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using TestPrep.Models;

namespace TestPrep.AxHelpers
{
    public class WebHelpers
    {
        /// <summary>
        /// Builds the results object.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="msg">The MSG.</param>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="total">The total.</param>
        /// <returns></returns>
        public static ResultObj BuildResponse(object data, string msg, bool success, int total = 0)
        {
            if (string.IsNullOrEmpty(msg)) msg = $"{total} record(s) found.";
            var results = new ResultObj
            {
                Data = data,
                Message = msg,
                Success = success,
                Total = total
            };

            return results;
        }

        /// <summary>
        /// Builds the erroe message
        /// </summary>
        /// <param name="exception">The ex.</param>
        /// <returns></returns>
        private static string ErrorMsg(Exception exception)
        {
            var validationException = exception as DbEntityValidationException;
            if (validationException != null)
            {
                var lines = validationException.EntityValidationErrors.Select(
                    x => new
                    {
                        name = x.Entry.Entity.GetType().Name.Split('_')[0],
                        errors = x.ValidationErrors.Select(y => y.PropertyName + ":" + y.ErrorMessage)
                    })
                                               .Select(x => $"{x.name} => {string.Join(",", x.errors)}");
                return string.Join("\r\n", lines);
            }

            var updateException = exception as DbUpdateException;
            if (updateException != null)
            {
                Exception innerException = updateException;
                while (innerException.InnerException != null) innerException = innerException.InnerException;
                if (innerException != updateException)
                {
                    if (innerException is SqlException)
                    {
                        var result = ProcessSqlExceptionMessage(innerException.Message);
                        if (!string.IsNullOrEmpty(result)) return result;
                    }
                }
                var entities = updateException.Entries.Select(x => x.Entity.GetType().Name.Split('_')[0])
                                              .Distinct()
                                              .Aggregate((a, b) => a + ", " + b);
                return ($"{innerException.Message} => {entities}");
            }

            var msg = exception.Message;
            if (exception.InnerException == null) return msg;
            msg = exception.InnerException.Message;

            if (exception.InnerException.InnerException == null) return msg;
            msg = exception.InnerException.InnerException.Message;

            if (exception.InnerException.InnerException.InnerException != null)
            {
                msg = exception.InnerException.InnerException.InnerException.Message;
            }
            if (msg.Contains("Object reference not set to an instance of an object"))
                msg = "Transaction failed. Please check the data";

            return msg;
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="exception">The ex.</param>
        /// <returns></returns>
        public static ResultObj ProcessException(Exception exception)
        {
            var msg = ErrorMsg(exception);
            return BuildResponse(null, msg, false, 0);
        }

        /// <summary>
        /// Processes the SQL exception message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string ProcessSqlExceptionMessage(string message)
        {

            if (message.Contains("unique index"))
                return "Sorry there is a unique constraint on this record.";
            return message.Contains("The DELETE statement conflicted with the REFERENCE constraint") ?
                "Sorry this cannot be deleted. It has been used in a previous transaction."
                : message;
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="values">The ASP.NET MVC model state values.</param>
        /// <returns></returns>
        public static ResultObj ProcessException(ICollection<ModelState> values)
        {
            var msg = values.SelectMany(modelState => modelState.Errors)
                .Aggregate("", (current, error) => current + error.ErrorMessage + "\n");
            return BuildResponse(null, msg, false, 0);
        }

        /// <summary>
        /// Processes the exception.
        /// </summary>
        /// <param name="identityResult">The identity result.</param>
        /// <returns></returns>
        public static ResultObj ProcessException(IdentityResult identityResult)
        {
            var msg = identityResult.Errors.Aggregate("", (current, error) => current + error + "\n");
            return BuildResponse(null, msg, false, 0);
        }


        public static Domain GetSubdomain()
        {
            var url = HttpContext.Current.Request.Url.Host;
            var subdomain = url.Contains(".") ? url.Split('.').FirstOrDefault() : "demo";
            string[] blacklist = { "www", "axoncubes" };
            if (blacklist.Contains(subdomain)) subdomain = "demo";

            return new Domain { Subdomain = subdomain, Url = url };
        }

        //public static UserModel GetUser(string userId, AppDbContext db)
        //{
        //    var us = db.Users.Include(x => x.Roles).First(x => x.Id == userId);
        //    var roleId = us.Roles.Select(x => x.RoleId).First();
        //    var role = db.Roles.First(x => x.Id == roleId);

        //    return new UserModel
        //    {
        //        UserName = us.UserName,
        //        FullName = us.FullName,
        //        Created = us.Created,
        //        Email = us.Email,
        //        DateOfBirth = us.DateOfBirth,
        //        PhoneNumber = us.PhoneNumber,
        //        Id = us.Id,
        //        IsActive = us.IsActive,
        //        Updated = us.Updated,
        //        Role = role.Name
        //    };
        //}

    }

    public class Domain
    {
        public string Url { get; set; }
        public string Subdomain { get; set; }
    }

    public class ResultObj
    {
        public long Total { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}