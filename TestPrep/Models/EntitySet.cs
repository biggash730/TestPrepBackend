using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TestPrep.AxHelpers;

namespace TestPrep.Models
{
    public interface IHasId
    {
        [Key]
        long Id { get; set; }
    }

    public interface ISecured
    {
        bool Locked { get; set; }
        bool Hidden { get; set; }
    }

    public interface IAuditable : IHasId
    {
        [Required]
        string CreatedBy { get; set; }
        [Required]
        string ModifiedBy { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
    }

    public class HasId : IHasId
    {
        public long Id { get; set; }
    }

    public class AuditFields : HasId, IAuditable, ISecured
    {
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }

    public class LookUp : AuditFields
    {
        [MaxLength(512), Required, Index(IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
    }
    public class LookUpx : AuditFields
    {
        [MaxLength(512), Required]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
    }

    public class Message : HasId
    {
        [MaxLength(256), Required]
        public string Recipient { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(128)]
        public string Subject { get; set; }
        [Required]
        public string Text { get; set; }
        public MessageStatus Status { get; set; } = MessageStatus.Pending;
        public MessageType Type { get; set; } = MessageType.SMS;
        [MaxLength(5000)]
        public string Response { get; set; }
        public DateTime TimeStamp { get; set; }
        [NotMapped]
        public string Attachment { get; set; }
    }

    #region User Management

    public class User : IdentityUser
    {
        public UserType UserType { get; set; }
        public Sex? Sex { get; set; }
        [Required, MaxLength(256)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public string Image { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; } = DateTime.Now;
        public bool Verified { get; set; } = false;
        public string VerificationCode { get; set; } = "";
        public bool HasValidSubscription { get; set; } = false;
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public virtual Profile Profile { get; set; }
        public long ProfileId { get; set; }
        public bool IsLoggedIn { get; set; } = false;
        public DateTime? LastActivityDate { get; set; }
        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public string ConfirmPassword { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            return userIdentity;
        }
        public virtual IList<PreviousPassword> PreviousUserPasswords { get; set; }
    }

    public class UserModel
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-20);
        public virtual Profile Profile { get; set; }
        public long ProfileId { get; set; }
    }

    public class UserLogin : HasId
    {
        public virtual User User { get; set; }
        public string UserId { get; set; }
        public DateTime LoginDate { get; set; } = DateTime.Now;
        public DateTime? LogoutDate { get; set; }
        public string DeviceIp { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public string Notes { get; set; } = "";
    }

    public class UserPushNotification : HasId
    {
        public virtual User User { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Message { get; set; } = "";
        public string Response { get; set; } = "";
        public bool IsSent { get; set; } = false;
    }

    public class LoginAttempt : HasId
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string DeviceIp { get; set; } = "";
        public string DeviceType { get; set; } = "";
        public long Count { get; set; }
        public string Notes { get; set; } = "";
        public DateTime Modified { get; set; }
    }

    public class AuditRecord : HasId
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string UserId { get; set; }
        public AuditType Type { get; set; }
        public string Data { get; set; }
    }

    public class PreviousPassword
    {
        public PreviousPassword()
        {
            CreateDate = DateTimeOffset.Now;
        }
        [Key, Column(Order = 0)]
        public string PasswordHash { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        [Key, Column(Order = 1)]
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }

    public class Profile : HasId
    {
        [Required, MaxLength(512), Index(IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Notes { get; set; }
        [MaxLength(500000)]
        public string Privileges { get; set; }
        public bool Locked { get; set; }
        public bool Hidden { get; set; }
        public List<User> Users { get; set; }
    }

    public class ResetRequest : HasId
    {
        public string Ip { get; set; } = "127.0.0.1";
        public DateTime Date { get; set; } = DateTime.Now;
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        public bool IsActive { get; set; } = false;
    }

    public class ResetModel
    {
        public string Token { get; set; }
        public string Password { get; set; }
    }
    #endregion

    #region Settings
    public enum AuditType
    {
        Insert,
        Update,
        Delete
    }

    public enum UserType
    {
        System,
        Mobile
    }

    public enum Sex
    {
        Male,
        Female
    }

    public enum MessageType
    {
        SMS,
        Email
    }

    public enum MessageStatus
    {
        Pending,
        Sent,
        Received,
        Failed
    }

    public enum SubscriptionStatus
    {
        Pending,
        Paid,
        Cancelled,
        Expired,
        Free,
        Processing,
        PaymentFailed
    }

    public enum SubscriptionPaymentStatus
    {
        Pending,
        Processing,
        Failed,
        Succeeded
    }

    #endregion

    #region App Models
    public class Kind : LookUp
    {
    }
    public class Type : LookUpx
    {
        public long KindId { get; set; }
        public virtual Kind Kind { get; set; }
    }
    public class Category : LookUpx
    {
        public long TypeId { get; set; }
        public virtual Type Type { get; set; }
    }
    public class TestsByCategories
    {
        public string Category { get; set; }
        public int TestCount { get; set; }
    }
    
    public class GetQuestionsModel
    {
        public List<long> CategoryIds { get; set; }
        public int NumberOfQuestions { get; set; } = 10;
        //public int NumberOfMinutes { get; set; } = 10;
    }

    public class MarkQuestionsModel
    {
        public List<Question> Questions { get; set; }
        public int TimeTaken { get; set; } = 0;
        public int Duration { get; set; } = 0;
    }

    public class Question : AuditFields
    {
        public string QuestionText { get; set; }
        public long CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public string Option1 { get; set; } = "";
        public string Option2 { get; set; } = "";
        public string Option3 { get; set; } = "";
        public string Option4 { get; set; } = "";
        public string Option5 { get; set; } = "";
        public string Answer { get; set; }
        public string Batch { get; set; }
        public string Reason { get; set; }
        [NotMapped]
        public string CategoryName { get; set; }
    }

    public class Result : AuditFields
    {
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime Date { get; set; }
        public int TotalQuestions { get; set; } = 0;
        public int TotalCorrect { get; set; } = 0;
        public string QuestionsList { get; set; }
        public string CorrectQuestions { get; set; }
        public string WrongQuestions { get; set; }
        public string CategoryIds { get; set; }
        public int Duration { get; set; } = 0;
        public int TimeTaken { get; set; } = 0;
        public string Categories { get; set; }
        public double Percentage
        {
            get { return (TotalCorrect / TotalQuestions) * 100; }
        }
    }

    public class ResultModel
    {
        public string CategoryIds { get; set; }
        public string Categories { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime Date { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalCorrect { get; set; }
        public int Duration { get; set; }
        public int TimeTaken { get; set; }
        public List<ReviewQuestion> ReviewQuestions { get; set; }
    }

    public class Settings
    {
        public string SupportEMail { get; set; }
        public string StripeSecretKey { get; set; }
        public string StripePublishableKey { get; set; }
        public string InfoEMail { get; set; }
        public string AppUrl { get; set; }
    }

    public class ReviewQuestion
    {
        public int QuestionNumber { get; set; }
        public string QuestionText { get; set; }
        public string SelectedOption { get; set; } = "";
        public string Answer { get; set; } = "";
        public string Option1 { get; set; } = "";
        public string Option2 { get; set; } = "";
        public string Option3 { get; set; } = "";
        public string Option4 { get; set; } = "";
        public string Option5 { get; set; } = "";
        public string Category { get; set; }
    }

    public class QuestionsUpload
    {
        public List<Question> Questions { get; set; }
        public long CategoryId { get; set; }
        public string Batch { get; set; }
    }

    public class SubscriptionPlan : AuditFields
    {
        public string Name { get; set; }
        public double Amount { get; set; }
        public int Duration { get; set; }
    }

    public class Subscription : AuditFields
    {
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public long SubscriptionPlanId { get; set; }
        public virtual SubscriptionPlan SubscriptionPlan { get; set; }
        public DateTime Date { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    }
    public class SubscriptionPayment : AuditFields
    {
        public long SubscriptionId { get; set; }
        public virtual Subscription Subscription { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reference { get; set; } = MessageHelpers.GenerateRandomString(16);
        public string Name { get; set; } = "";
        public string Number { get; set; }
        public string Network { get; set; }// mtn-gh, vodafone-gh, tigo-gh, airtel-gh
        public double Amount { get; set; }
        public string Token { get; set; }
        public bool FeesOnCustomer { get; set; } = true;
        public string TransactionId { get; set; }
        public SubscriptionPaymentStatus Status { get; set; } = SubscriptionPaymentStatus.Pending;
        public string Response { get; set; }
    }

    public class DashboardSummaries
    {
        public int Users { get; set; } = 0;
        public int Categories { get; set; } = 0;
        public int Tests { get; set; } = 0;
    }

    public class SupportMessage : HasId
    {
        public DateTime Created { get; set; } = DateTime.Now;
        public string Message { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
        public string Device { get; set; }
        public string Platform { get; set; }
        public string Uuid { get; set; }
    }

    public class Notification : HasId
    {
        public bool Opened { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }

    public class Filter
    {
        [DefaultValue(1)]
        public int Page { get; set; } = 0;
        [DefaultValue(25)]
        public int Size { get; set; } = 0;
        public int Skip()
        {
            return Page * Size;
        }
        public long Id { get; set; } = 0;
        public long SubjectId { get; set; } = 0;
        public long TopicId { get; set; } = 0;
        public long CategoryId { get; set; } = 0;
        public string RoleId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public string QuestionText { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public long ProductId { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = false;
        public string Type { get; set; } = "";
    }
    #endregion
}