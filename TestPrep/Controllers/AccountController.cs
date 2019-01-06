using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using hubtelapi_dotnet_v1.Hubtel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using TestPrep.AxHelpers;
using TestPrep.DataAccess.Filters;
using TestPrep.DataAccess.Repositories;
using TestPrep.Extensions;
using TestPrep.IdentityExtensions;
using TestPrep.Models;
using TestPrep.Providers;
using TestPrep.Results;
using WebGrease.Css.Extensions;
using Message = TestPrep.Models.Message;

namespace TestPrep.Controllers
{
    [Authorize]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        //private ApplicationUserManager _userManager;

        private readonly UserRepository _userRepo = new UserRepository();


        //public AccountController(ApplicationUserManager userManager,
        //    ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        //{
        //    UserManager = userManager;
        //    UserManager.PasswordValidator = new CustomPasswordValidator();
        //    AccessTokenFormat = accessTokenFormat;
        //}
        public AccountController()
            : this(new ApplicationUserManager())
        {
        }

        public AccountController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
            UserManager.UserValidator = new UserValidator<User>(UserManager)
            {
                AllowOnlyAlphanumericUserNames =
                    false
            };

            UserManager.PasswordValidator = new CustomPasswordValidator();
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            UserManager.PasswordValidator = new CustomPasswordValidator();
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager { get; }




        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; }

        [Route("logon")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResultObj> UserLogon(string phoneNumber)
        {
            using (var db = new AppDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var role = new ProfileRepository().GetProfileByName("User", db);
                        if (role == null) throw new Exception("User profile Issue!! System Related");

                        var token = MessageHelpers.GenerateRandomNumber(4);
                        //Input validations
                        var number = phoneNumber;

                        if (!number.StartsWith("0")) throw new Exception("Please check the phone number");
                        if (number.Length < 10 || number.Length > 10) throw new Exception("The phone number is not a valid phone number");

                        var usr = db.Users.FirstOrDefault(x => x.UserName == number);
                        if (usr == null)
                        {
                            usr = new User
                            {
                                UserName = number,
                                PhoneNumber = number,
                                Email = "",
                                ProfileId = role.Id,
                                Name = "User",
                                UserType = UserType.Mobile,
                                Created = DateTime.UtcNow,
                                Updated = DateTime.UtcNow,
                                VerificationCode = token,
                                Verified = false,
                                HasValidSubscription = false
                            };
                            var password = number + "xxx" + token;
                            var identityResult = await UserManager.CreateAsync(usr, password);
                            if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);
                            db.SaveChanges();

                            //Add Roles in selected Role to user
                            if (!string.IsNullOrEmpty(role.Privileges))
                            {
                                var privs = role.Privileges.Split(',');
                                privs.ForEach(r => UserManager.AddToRole(usr.Id, r.Trim()));
                            }
                        }
                        else
                        {
                            usr.VerificationCode = token;
                            usr.Verified = false;
                        }

                        //Check subscription
                        if (usr.SubscriptionStartDate == null)
                        {
                            var date = DateTime.Now;
                            var plan = db.SubscriptionPlans.FirstOrDefault(x => x.Amount == 0);
                            if (plan == null) throw new Exception("System Error");

                            var sub = new Subscription
                            {
                                Status = SubscriptionStatus.Free,
                                UserId = usr.Id,
                                SubscriptionPlanId = plan.Id,
                                Date = date,
                                CreatedAt = date,
                                ModifiedAt = date,
                                CreatedBy = usr.UserName,
                                ModifiedBy = usr.UserName
                            };
                            db.Subscriptions.Add(sub);

                            var user = db.Users.First(x => x.Id == usr.Id);
                            user.HasValidSubscription = true;
                            user.SubscriptionStartDate = date;
                            user.SubscriptionEndDate = date.AddDays(plan.Duration);
                        }

                        //send the token
                        var msg = $"Hello {usr.Name}, Your Test Prep verification code is {token}. Enjoy!!";
                        MessageHelpers.AddMessage("Verification Code", msg, usr, db);
                        transaction.Commit();
                        return WebHelpers.BuildResponse(null, "Please verify your account to continue",
                            true, 1);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return WebHelpers.ProcessException(ex);
                    }
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("verify")]
        public async Task<ResultObj> VerifyAccount(VerifyModel rm)
        {
            using (var db = new AppDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {

                    try
                    {
                        var today = DateTime.Now;

                        var user = db.Users.FirstOrDefault(x => x.UserName == rm.PhoneNumber);
                        if (user == null) throw new Exception("Please check your Phone Number");
                        if (user.VerificationCode != rm.Code) throw new Exception("Please check your code");

                        user.Verified = true;
                        user.Updated = DateTime.Now;
                        db.SaveChanges();

                        var password = user.UserName + "xxx" + user.VerificationCode;

                        var role = new ProfileRepository().GetProfileByName("User");
                        if (role == null) throw new Exception("User profile Issue!! System Related");
                        if (user.ProfileId != role.Id) throw new Exception("Profile Error:: Invalid profile");
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

                        var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                        authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);

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
                            user.Image,
                            Sex = user.Sex?.ToString(),
                            user.Updated,
                            user.Created,
                            user.LastActivityDate,
                            user.IsLoggedIn,
                            user.ProfileId,
                            user.HasValidSubscription,
                            user.SubscriptionEndDate,
                            user.SubscriptionStartDate,
                            Profile = user.Profile?.Name,
                            Role = new
                            {
                                user.Profile?.Id,
                                user.Profile?.Name,
                                Privileges = user.Profile?.Privileges.Split(',')
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
                        //send message
                        //var plan = db.SubscriptionPlans.First(x => x.Amount == 0);
                        var msg = $"Hello {user.Name}, Test Prep Verification is Successful. Happy Preparation!!";
                        MessageHelpers.AddMessage("Successful Verification", msg, user, db);
                        
                        transaction.Commit();
                        return WebHelpers.BuildResponse(data, "Verification Successful", true, 0);
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return WebHelpers.ProcessException(e);
                    }
                }
            }
        }


        [Route("signupxx")]
        [AllowAnonymous]
        public async Task<ResultObj> UserSignUp(RegisterModel model)
        {
            using (var db = new AppDbContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (!ModelState.IsValid) return WebHelpers.ProcessException(ModelState.Values);
                        if (model.Password != model.ConfirmPassword) throw new Exception("The password and confirm password should be the same.");
                        var role = new ProfileRepository().GetProfileByName("User", db);
                        if (role == null) throw new Exception("User profile Issue!! System Related");

                        var token = MessageHelpers.GenerateRandomNumber(6);
                        //Input validations
                        var number = model.PhoneNumber;

                        if (!number.StartsWith("0")) throw new Exception("Please check the phone number");
                        if (number.Length < 10 || number.Length > 10) throw new Exception("The phone number is not a valid phone number");

                        if (string.IsNullOrWhiteSpace(model.Name)) throw new Exception("Please provide a valid name");

                        if (!string.IsNullOrWhiteSpace(model.Email) && !model.Email.IsEmail()) throw new Exception("The email address provided is not a valid email");

                        var user = new User
                        {
                            UserName = number,
                            PhoneNumber = number,
                            Email = model.Email,
                            ProfileId = role.Id,
                            Name = model.Name,
                            Sex = model.Sex,
                            UserType = UserType.Mobile,
                            Created = DateTime.UtcNow,
                            Updated = DateTime.UtcNow,
                            VerificationCode = token,
                            Verified = false,
                            HasValidSubscription = false
                        };

                        var identityResult = await UserManager.CreateAsync(user, model.Password);
                        if (!identityResult.Succeeded) return WebHelpers.ProcessException(identityResult);
                        db.SaveChanges();

                        //Add Roles in selected Role to user
                        if (!string.IsNullOrEmpty(role.Privileges))
                        {
                            var privs = role.Privileges.Split(',');
                            privs.ForEach(r => UserManager.AddToRole(user.Id, r.Trim()));
                        }

                        //send the token
                        var msg = new Message
                        {
                            Text =
                                $"Hello {model.Name}, Thank you for signing up for NM Quiz Prep. Your Account Verification Code is {token}. Enjoy!!",
                            Subject = "Verification Code",
                            Recipient = user.PhoneNumber,
                            TimeStamp = DateTime.Now,
                            Status = MessageStatus.Pending
                        };
                        db.Messages.Add(msg);
                        db.SaveChanges();
                        transaction.Commit();
                        return WebHelpers.BuildResponse(null, "Registration Successful, Please verify your account.",
                            true, 1);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return WebHelpers.ProcessException(ex);
                    }
                }
            }
        }

        

        

        [AllowAnonymous]
        [HttpGet]
        [Route("resendverification")]
        public ResultObj ResendVerificationCode(string phone)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(x => x.UserName == phone);
                    if (user == null) throw new Exception("Please check your Phone number");
                    if (user.Verified) throw new Exception("You have already verified your account. Please login.");
                    //resend the token
                    var msg = $"Hello {user.Name}, Your verification code is {user.VerificationCode}. Enjoy!!";
                    MessageHelpers.AddMessage("Verification Code", msg, user, db);
                    db.SaveChanges();
                    return WebHelpers.BuildResponse(null, "Verification code has been resent to your phone number", true, 1);
                }
            }
            catch (Exception e)
            {
                return WebHelpers.ProcessException(e);
            }
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
                    var msg = $"You have requested to reset your Password. Your reset token is {newRecord.Token}. Please ignore this message if you did not request a password reset.";
                    MessageHelpers.AddMessage("Password Reset", msg, existing, db);
                    db.SaveChanges();

                    return WebHelpers.BuildResponse(null, "Password reset token has been sent to your phone number.", true, 1);
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
                    user.Image,
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
                

                if (string.IsNullOrWhiteSpace(model.Name)) throw new Exception("Please provide a valid name");

                if (string.IsNullOrWhiteSpace(model.Email)) throw new Exception("The email address provided is not a valid email");

                var role = new ProfileRepository().Get(model.ProfileId);

                var user = db.Users.First(x => x.Id == usr.Id);
                user.Name = model.Name;
                user.Sex = model.Sex;
                user.Updated = DateTime.UtcNow;
                user.Email = model.Email;

                var msgg = "Profile Updated Successfully.";

                /*if (user.UserName != number)
                {
                    var token = MessageHelpers.GenerateRandomNumber(6);
                    user.UserName = number;
                    user.PhoneNumber = number;
                    user.Verified = false;
                    user.VerificationCode = token;
                    msgg = "Your phone number has changed, please verify your phone number";

                    //send the token
                    var msg = new Message
                    {
                        Text =
                            $"Hello {model.Name}, Your phone number has changed. Your New Verification Code is {token}. Enjoy!!",
                        Subject = "New Verification Code",
                        Recipient = user.PhoneNumber,
                        TimeStamp = DateTime.Now,
                        Status = MessageStatus.Pending
                    };
                    db.Messages.Add(msg);
                }*/
                db.SaveChanges();

                results = WebHelpers.BuildResponse(null, msgg, true, 1);
            }
            catch (Exception ex)
            {
                results = WebHelpers.ProcessException(ex);
            }

            return results;
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

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
