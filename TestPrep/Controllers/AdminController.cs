using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using TestPrep.AxHelpers;
using TestPrep.DataAccess.Filters;
using TestPrep.DataAccess.Repositories;
using TestPrep.Extensions;
using TestPrep.Models;

namespace TestPrep.Controllers
{
    [RoutePrefix("api/admin")]
    public class AdminController : ApiController
    {
        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultObj> Login(LoginModel model)
        {
            try
            {
                var today = DateTime.Now;
                using (var db = new AppDbContext())
                {
                    var userMan = new UserManager<User>(new UserStore<User>(db));
                    if (!ModelState.IsValid) throw new Exception("Please check the login details");

                    /*//check attemp counts
                var checkAttempt =
                        db.LoginAttempts.Where(
                            x =>
                                (x.Date.Year == today.Year && x.Date.Month == today.Month && x.Date.Day == today.Day) &&
                                x.DeviceIp == HttpContext.Current.Request.Url.Host && x.DeviceType == HttpContext.Current.Request.UserAgent).FirstOrDefault();
                if (checkAttempt?.Count >= 5 && today < checkAttempt.Modified.AddMinutes(15))
                {
                    checkAttempt.Modified = DateTime.Now.AddMinutes(15);
                    db.SaveChanges();
                    throw new Exception(
                        "Eeeiii, Didn't I tell you to try again after 15 minutes? Now try again after another 15 minutes. Cheers!!");
                }*/

                    var user = await userMan.FindAsync(model.UserName, model.Password);

                    if (user == null) throw new Exception("Please check the login details");
                    //if (!user.Verified) throw new Exception("You need to verify your account before you can logins");
                    if (!user.IsActive) throw new Exception("This user account has been deactivated");
                    if (user.UserType != UserType.System) throw new Exception("This action is not allowed for this user type");

                    var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                    authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    var identity =
                        await userMan.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe },
                        identity);

                    var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
                    var token = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
                    var data = new
                    {
                        UserType = user.UserType.ToString(),
                        user.Id,
                        Username = user.UserName,
                        user.Name,
                        user.PhoneNumber,
                        user.Email,
                        Sex = user.Sex.ToString(),
                        user.Updated,
                        user.Created,
                        user.LastActivityDate,
                        user.IsLoggedIn,
                        user.ProfileId,
                        user.HasValidSubscription,
                        user.SubscriptionEndDate,
                        user.SubscriptionStartDate,
                        Profile = user.Profile.Name,
                        Role = new
                        {
                            user.Profile.Id,
                            user.Profile.Name,
                            Privileges = user.Profile.Privileges.Split(',')
                        },
                        token
                    };

                    //log user login
                    var newLogin = new UserLogin
                    {
                        DeviceIp = HttpContext.Current.Request.Url.Host,
                        DeviceType = HttpContext.Current.Request.UserAgent,
                        LoginDate = DateTime.Now,
                        UserId = user.Id
                    };
                    db.UserLogins.Add(newLogin);
                    db.SaveChanges();

                    //reset login attempts
                    var attempt =
                            db.LoginAttempts.FirstOrDefault(
                                x =>
                                    (x.Date.Year == today.Year && x.Date.Month == today.Month && x.Date.Day == today.Day) &&
                                    x.DeviceIp == HttpContext.Current.Request.Url.Host && x.DeviceType == HttpContext.Current.Request.UserAgent);
                    if (attempt != null)
                    {
                        attempt.Count = 0;
                        attempt.Modified = DateTime.Now;
                        attempt.Notes = $"Login Attemps was resetted on {today}";
                        db.SaveChanges();
                    }

                    return WebHelpers.BuildResponse(data, "Login Successfull", true, 0);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("Logout")]
        public ResultObj Logout()
        {
            try
            {
                var db = new AppDbContext();
                var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
                authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                //log user out
                var exst =
                    db.UserLogins.Where(x => x.LogoutDate == null).OrderByDescending(x => x.LoginDate).FirstOrDefault();
                if (exst != null)
                {
                    exst.LogoutDate = DateTime.Now;
                }
                db.SaveChanges();

                return WebHelpers.BuildResponse(new { }, "User Logged Out", true, 0);
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [Route("GetUsers")]
        public ResultObj GetUsers()
        {
            try
            {
                var db = new AppDbContext();
                var data = db.Users.Include(x => x.Profile).Where(x => x.ProfileId > 0).ToList()
                    .Select(user => new
                    {
                        UserType = user.UserType.ToString(),
                        user.Id,
                        Username = user.UserName,
                        user.Name,
                        user.PhoneNumber,
                        user.Email,
                        Sex = user.Sex.ToString(),
                        user.Updated,
                        user.Created,
                        user.LastActivityDate,
                        user.IsLoggedIn,
                        user.ProfileId,
                        user.HasValidSubscription,
                        user.SubscriptionEndDate,
                        user.SubscriptionStartDate,
                        Profile = user.Profile.Name
                    }).ToList();
                return WebHelpers.BuildResponse(data, "", true, data.Count);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        [Route("queryUsers")]
        public ResultObj QueryUsers(UserFilter filter)
        {
            try
            {
                var db = new AppDbContext();
                var data = filter.BuildQuery(db.Users.Include(x => x.Profile)).Where(x => x.ProfileId > 0).ToList();
                var total = data.Count();
                if (filter.Pager.Page > 0)
                {
                    data = data.Skip(filter.Pager.Skip()).Take(filter.Pager.Size).ToList();
                }
                var res = data.Select(user => new
                {
                    UserType = user.UserType.ToString(),
                    user.Id,
                    Username = user.UserName,
                    user.Name,
                    user.PhoneNumber,
                    user.Email,
                    Sex = user.Sex.ToString(),
                    user.Updated,
                    user.Created,
                    user.LastActivityDate,
                    user.IsLoggedIn,
                    user.ProfileId,
                    user.HasValidSubscription,
                    user.SubscriptionEndDate,
                    user.SubscriptionStartDate,
                    Profile = user.Profile.Name
                }).ToList();
                return WebHelpers.BuildResponse(res, "", true, total);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        [HttpGet]
        [Route("DeleteUser")]
        public ResultObj DeleteUser(string id)
        {
            ResultObj results;
            try
            {
                var db = new AppDbContext();
                var user = db.Users.First(x => x.Id == id);
                user.IsDeleted = true;
                user.Updated = DateTime.Now;
                db.SaveChanges();
                results = WebHelpers.BuildResponse(id, "User Deleted Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        [HttpGet]
        [Route("deactivateuser")]
        public ResultObj Deactivate(string id)
        {
            ResultObj results;
            try
            {
                var db = new AppDbContext();
                var user = db.Users.First(x => x.Id == id);
                user.IsActive = false;
                user.Updated = DateTime.Now;
                db.SaveChanges();
                results = WebHelpers.BuildResponse(id, "User Deactivated Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        [HttpGet]
        [Route("activateuser")]
        public ResultObj Activate(string id)
        {
            ResultObj results;
            try
            {
                var db = new AppDbContext();
                var user = db.Users.First(x => x.Id == id);
                user.IsActive = true;
                user.Updated = DateTime.Now;
                db.SaveChanges();
                results = WebHelpers.BuildResponse(id, "User Activated Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }

        [HttpGet]
        [Route("getuserdetails")]
        public ResultObj GetUserDetails(string id)
        {
            try
            {
                var db = new AppDbContext();
                var data = db.Users.Where(x => x.Id == id).ToList()
                    .Select(user => new
                    {
                        UserType = user.UserType.ToString(),
                        user.Id,
                        Username = user.UserName,
                        user.Name,
                        user.PhoneNumber,
                        user.Email,
                        Sex = user.Sex.ToString(),
                        user.Updated,
                        user.Created,
                        user.LastActivityDate,
                        user.IsLoggedIn,
                        user.ProfileId,
                        user.HasValidSubscription,
                        user.SubscriptionEndDate,
                        user.SubscriptionStartDate,
                        Profile = user.Profile.Name,
                        Role = new
                        {
                            user.Profile.Id,
                            user.Profile.Name,
                            Privileges = user.Profile.Privileges.Split(',')
                        }
                    }).FirstOrDefault();
                return WebHelpers.BuildResponse(data, "Successful", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<ResultObj> ChangePassword(ChangePasswordBindingModel model)
        {
            try
            {
                var today = DateTime.Now;
                using (var db = new AppDbContext())
                {
                    var userMan = new UserManager<User>(new UserStore<User>(db));
                    if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);

                    var result = await userMan.ChangePasswordAsync(User.Identity.GetUserId(),
                        model.OldPassword, model.NewPassword);

                    return !result.Succeeded ? WebHelpers.ProcessException(result)
                        : WebHelpers.BuildResponse(model, "Password changed sucessfully.", true, 1);
                }
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }

        }

        [HttpPost]
        [Route("CreateUser")]
        public async Task<ResultObj> CreateUser(User model)
        {
            try
            {
                var db = new AppDbContext();
                var userMan = new UserManager<User>(new UserStore<User>(db));
                var cuser = User.Identity.AsAppUser().Result;
                var cUserPrivs = db.Profiles.First(x => x.Id == cuser.ProfileId).Privileges;
                if (!cUserPrivs.Any()) throw new Exception("Sorry you do not have the privilege to create a user.");
                if (!cUserPrivs.Contains(Privileges.CanCreateUser)) throw new Exception("Sorry you do not have the privilege to create a user.");
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var role = new ProfileRepository().Get(model.ProfileId);

                var token = MessageHelpers.GenerateRandomNumber(6);

                var user = new User
                {
                    UserType = UserType.System,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Sex = model.Sex,
                    Email = model.Email,
                    ProfileId = role.Id,
                    Name = model.Name,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    VerificationCode = token,
                    Verified = true,
                    HasValidSubscription = false,
                };

                var identityResult = await userMan.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);

                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ToList().ForEach(r => userMan.AddToRole(user.Id, r.Trim()));
                }
                db.SaveChanges();

                return WebHelpers.BuildResponse(user, "User Created Successfully", true, 1);

            }
            catch (Exception ex)
            {
                return WebHelpers.ProcessException(ex);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("resetuserpassword")]
        public async Task<ResultObj> ResetUserPassword(ResetPasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                var db = new AppDbContext();
                var userMan = new UserManager<User>(new UserStore<User>(db));

                var user = await userMan.FindByNameAsync(model.UserName);
                if (user == null) throw new Exception("Please check the username.");


                //reset old passwords
                var res = await userMan.RemovePasswordAsync(user.Id);
                if (!res.Succeeded) return WebHelpers.ProcessException(res);
                var result = await userMan.AddPasswordAsync(user.Id, model.NewPassword);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First());
                }
                return !result.Succeeded
                    ? WebHelpers.ProcessException(result)
                    : WebHelpers.BuildResponse(model, "Password reset was sucessful.", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }

        }

        [HttpGet]
        [Route("getprofile")]
        public ResultObj GetProfile()
        {
            try
            {
                var usr = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                var user = db.Users.FirstOrDefault(x => x.Id == usr.Id);
                if (user == null) throw new Exception("Unknown User");
                var data = new
                {
                    UserType = user.UserType.ToString(),
                    user.Id,
                    Username = user.UserName,
                    user.Name,
                    user.PhoneNumber,
                    user.Email,
                    Sex = user.Sex.ToString(),
                    user.Updated,
                    user.Created,
                    user.LastActivityDate,
                    user.IsLoggedIn,
                    user.ProfileId,
                    user.HasValidSubscription,
                    user.SubscriptionEndDate,
                    user.SubscriptionStartDate,
                    Profile = user.Profile.Name,
                };
                return WebHelpers.BuildResponse(data, "Successful", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }
        }

        [Route("SetPassword")]
        [HttpPost]
        public async Task<ResultObj> SetPassword(SetPasswordBindingModel model)
        {
            try
            {
                var user = User.Identity.AsAppUser().Result;
                if (!ModelState.IsValid) throw new Exception("Please check the new password");
                var db = new AppDbContext();
                var userMan = new UserManager<User>(new UserStore<User>(db));

                IdentityResult result = await userMan.AddPasswordAsync(user.Id, model.NewPassword);

                if (!result.Succeeded) throw new Exception(result.Errors.ToString());

                return WebHelpers.BuildResponse(null, "Password has been set successfully.", true, 1);
            }
            catch (Exception exception)
            {
                return WebHelpers.ProcessException(exception);
            }

        }

        [HttpPost]
        [Route("updateprofile")]
        public ResultObj UpdateProfile(User model)
        {
            ResultObj results;
            try
            {
                var usr = User.Identity.AsAppUser().Result;
                var db = new AppDbContext();
                var userMan = new UserManager<User>(new UserStore<User>(db));
                //Input validations
                var number = model.PhoneNumber;

                if (!number.StartsWith("0")) throw new Exception("Please check the phone number");
                if (number.Length < 10 || number.Length > 10) throw new Exception("The phone number is not a valid phone number");

                if (string.IsNullOrWhiteSpace(model.Name)) throw new Exception("Please provide a valid name");

                if (!string.IsNullOrWhiteSpace(model.Email)) throw new Exception("The email address provided is not a valid email");

                var role = new ProfileRepository().Get(model.ProfileId);

                var user = db.Users.First(x => x.Id == usr.Id);
                user.Name = model.Name;
                user.Sex = model.Sex;
                user.UserType = model.UserType;
                user.Updated = DateTime.UtcNow;
                user.Email = model.Email;
                user.ProfileId = model.ProfileId;
                db.SaveChanges();

                //Add Roles in selected Role to user
                if (!string.IsNullOrEmpty(role.Privileges))
                {
                    role.Privileges.Split(',').ToList().ForEach(r => userMan.AddToRole(user.Id, r.Trim()));
                }
                db.SaveChanges();

                results = WebHelpers.BuildResponse(null, "Profile Updated Successfully.", true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
        }



        [AllowAnonymous]
        [HttpPost]
        [Route("resetpassword")]
        public ResultObj ResetPassword(ResetModel rm)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var userMan = new UserManager<User>(new UserStore<User>(db));
                    var existing = db.ResetRequests.FirstOrDefault(x => x.Token == rm.Token && x.IsActive);
                    if (existing == null) throw new Exception("Password reset was not complete");

                    var us =
                        db.Users.FirstOrDefault(x => x.UserName == existing.PhoneNumber);
                    if (us == null) throw new Exception("System Error");
                    var result = userMan.RemovePassword(us.Id);
                    if (result.Succeeded)
                    {
                        var res = userMan.AddPassword(us.Id, rm.Password);
                        if (res.Succeeded) existing.IsActive = false;
                        else throw new Exception(string.Join(", ", res.Errors));
                    }
                    db.SaveChanges();
                    return WebHelpers.BuildResponse(null, "Password Reset Successful", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("requestpasswordreset")]
        public ResultObj RequestPasswordReset(string phone)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var existing = db.Users.FirstOrDefault(x => x.UserName == phone);
                    if (existing == null) throw new Exception("Please check your Phone Number!!");
                    //deactivate all other requests
                    var resReqs = db.ResetRequests.Where(x => x.PhoneNumber == phone && x.IsActive).ToList();
                    foreach (var r in resReqs)
                    {
                        r.IsActive = false;
                    }
                    db.SaveChanges();
                    var newRecord = new ResetRequest
                    {
                        PhoneNumber = existing.PhoneNumber,
                        Token = MessageHelpers.GenerateRandomNumber(6),
                        Date = DateTime.Now,
                        Ip = Request?.Headers?.Referrer?.AbsoluteUri,
                        IsActive = true
                    };
                    db.ResetRequests.Add(newRecord);
                    db.SaveChanges();

                    // create a password reset entry
                    var msg = new Message
                    {
                        Text =
                            $"You have requested to reset your Password. Your reset token is {newRecord.Token}. Please ignore this message if you did not request a password reset.",
                        Subject = "Password Reset",
                        Recipient = existing.PhoneNumber,
                        TimeStamp = DateTime.Now
                    };
                    db.Messages.Add(msg);
                    db.SaveChanges();

                    return WebHelpers.BuildResponse(null, "Password reset token has been sent to your phone number.", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [Route("dashboardsummaries")]
        [HttpGet]
        public ResultObj DashboardSummaries()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var dash = new DashboardSummaries
                    {
                        Users = db.Users.Where(x => x.IsActive).ToList().Count(),
                        Categories = db.Categories.Where(x => x.IsActive).ToList().Count(),
                        Tests = db.Results.Where(x => x.IsActive).ToList().Count()
                    };
                    db.SaveChanges();
                    return WebHelpers.BuildResponse(dash, "Loaded Successfully", true);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

        [HttpGet]
        [Route("GetTestsByCategories")]
        public ResultObj GetTestsByCategories()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var allCatIds = new List<long>();
                    var resultsCatIds = db.Results.Where(x => !x.IsDeleted && x.IsActive).Select(x => x.CategoryIds).ToList();
                    foreach (var rc in resultsCatIds)
                    {
                        allCatIds.AddRange(rc.Split(',').Select(long.Parse));
                    }
                    var cats = db.Categories.Where(x => x.IsActive && !x.IsDeleted).ToList();
                    var data = (from c in cats let cnt = allCatIds.Where(x => x == c.Id).ToList().Count() select new TestsByCategories { Category = c.Name, TestCount = cnt }).ToList();
                    data = data.Where(x => x.TestCount > 0).ToList();
                    return WebHelpers.BuildResponse(data, "Loaded successfully", true, data.Count());
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
        }

    }
}