using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TestPrep.AxHelpers;
using TestPrep.Models;

namespace TestPrep.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Models.AppDbContext>
    {
        public UserManager<User> UserManager { get; private set; }

        public Configuration()
            : this(new UserManager<User>(new UserStore<User>(new AppDbContext())))
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        public Configuration(UserManager<User> userManager) { UserManager = userManager; }

        protected override void Seed(AppDbContext context)
        {
            #region Roles [Privileges]
            var roles = new List<IdentityRole>
                        {
                            new IdentityRole {Name = Privileges.CanViewDashboard},
                            new IdentityRole {Name = Privileges.CanViewReport},
                            new IdentityRole {Name = Privileges.CanViewSetting},
                            new IdentityRole {Name = Privileges.CanViewAdministration},
                            new IdentityRole {Name = Privileges.CanViewUser},
                            new IdentityRole {Name = Privileges.CanCreateUser},
                            new IdentityRole {Name = Privileges.CanUpdateUser},
                            new IdentityRole {Name = Privileges.CanDeleteUser},
                            new IdentityRole {Name = Privileges.CanViewRole},
                            new IdentityRole {Name = Privileges.CanCreateRole},
                            new IdentityRole {Name = Privileges.CanUpdateRole},
                            new IdentityRole {Name = Privileges.CanDeleteRole}
                        };
            var userRoles = new List<IdentityRole>
                        {
                            new IdentityRole {Name = Privileges.IsUser}
                        };

            roles.ForEach(r => context.Roles.AddOrUpdate(q => q.Name, r));
            userRoles.ForEach(r => context.Roles.AddOrUpdate(q => q.Name, r));
            var a = "";
            roles.ForEach(q => a += q.Name + ",");
            #endregion

            #region App Roles
            var adminProfile = new Profile
            {
                Name = "Administrator",
                Notes = "Administrator Role",
                Privileges = a.Trim(','),
                Locked = true
            };
            var b = "";
            userRoles.ForEach(q => b += q.Name + ",");
            var user = new Profile
            {
                Name = "User",
                Notes = "User Role",
                Privileges = b.Trim(','),
                Locked = true
            };
            context.Profiles.AddOrUpdate(x => x.Name, user);
            #endregion

            #region Users
            var userManager = new UserManager<User>(new UserStore<User>(context))
            {
                UserValidator = new UserValidator<User>(UserManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                }
            };

            //Admin User
            if (UserManager.FindByNameAsync("admin").Result == null)
            {
                var res = userManager.CreateAsync(new User
                {
                    Name = "Application Admin",
                    Profile = adminProfile,
                    UserName = "administrator",
                    PhoneNumber = "0207985828",
                    Email = "biggash730@gmail.com",
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    Verified = true,
                    IsActive = true,
                    VerificationCode = "123456",
                    LastActivityDate = DateTime.Now,
                    UserType = UserType.System,
                    Sex = Sex.Male,
                    HasValidSubscription = false
                }, "p@ssword1");

                if (res.Result.Succeeded)
                {
                    var userId = userManager.FindByNameAsync("admin").Result.Id;
                    roles.ForEach(q => userManager.AddToRole(userId, q.Name));
                }
            }

            #endregion

            #region Update Roles
            roles.ForEach(q => context.Roles.AddOrUpdate(q));
            #endregion

            context.SaveChanges();
            base.Seed(context);
        }
    }
}
