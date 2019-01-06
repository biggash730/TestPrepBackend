using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using TestPrep.IdentityExtensions;
using TestPrep.Models;

namespace TestPrep
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<User>
    {
        private int _previousPasswordCount = SetupConfig.Setting.PasswordPolicies.PreviousPasswordCount;
        public ApplicationUserManager()
            : base(new ApplicationUserStore(new AppDbContext()))
        {
            PasswordValidator = new CustomPasswordValidator();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager();
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<User>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new CustomPasswordValidator();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<User>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = TimeSpan.FromHours(24)
                    };
            }
            return manager;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            if (await IsPreviousPassword(userId, newPassword))
            {
                return await Task.FromResult(IdentityResult.Failed("Cannot reuse old password"));
            }
            var result = await base.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (result.Succeeded)
            {
                var user = await FindByIdAsync(userId);
                var store = Store as ApplicationUserStore;
                await store.AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(newPassword));
            }

            return result;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            if (await IsPreviousPassword(userId, newPassword))
            {
                return await Task.FromResult(IdentityResult.Failed("Cannot reuse old password"));
            }
            var result = await base.ResetPasswordAsync(userId, token, newPassword);
            if (!result.Succeeded) return result;
            var user = await FindByIdAsync(userId);
            var store = Store as ApplicationUserStore;
            await store.AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(newPassword));
            return result;
        }

        public override async Task<IdentityResult> AddPasswordAsync(string userId, string newPassword)
        {
            if (await IsPreviousPassword(userId, newPassword))
            {
                return await Task.FromResult(IdentityResult.Failed("Cannot reuse old password"));
            }
            var result = await base.AddPasswordAsync(userId, newPassword);
            if (!result.Succeeded) return result;
            var user = await FindByIdAsync(userId);
            var store = Store as ApplicationUserStore;
            await store.AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(newPassword));
            return result;
        }

        private async Task<bool> IsPreviousPassword(string userId, string newPassword)
        {
            var user = await FindByIdAsync(userId);
            if (!user.PreviousUserPasswords.Any()) return false;
            return user.PreviousUserPasswords.OrderByDescending(x => x.CreateDate).
                Select(x => x.PasswordHash)
                .Take(_previousPasswordCount)
                .Where(x => PasswordHasher.VerifyHashedPassword(x, newPassword) != PasswordVerificationResult.Failed
                ).Any();
        }

        //return user.PreviousUserPasswords.
        //    OrderByDescending(x => x.CreateDate).Select(x => x.PasswordHash).Take(_previousPasswordCount
        //    ).Any(x => PasswordHasher.VerifyHashedPassword(x, newPassword) != PasswordVerificationResult.Failed);
    }
}
