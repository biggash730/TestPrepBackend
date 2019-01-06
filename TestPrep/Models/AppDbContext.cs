using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using TestPrep.AxHelpers;

namespace TestPrep.Models
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext() : base("AppDbContext") { }
        public DbSet<ResetRequest> ResetRequests { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<UserPushNotification> UserPushNotifications { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Kind> Kinds { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPayment> subscriptionPayments { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Migrations.Configuration>());
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .OfType<IAuditable>())
            {
                entry.CreatedAt = DateTime.UtcNow;
                entry.ModifiedAt = DateTime.UtcNow;
            }

            foreach (var entry in ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<IAuditable>())
            {
                entry.ModifiedAt = DateTime.UtcNow;
            }
            return base.SaveChanges();
        }
    }
}