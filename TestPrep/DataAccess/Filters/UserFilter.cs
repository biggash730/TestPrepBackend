using System;
using System.Data.Entity;
using System.Linq;
using TestPrep.Models;

namespace TestPrep.DataAccess.Filters
{
    public class PeriodicReportsFilter
    {
        public DateTime? StartDate;
        public DateTime? EndDate;
    }

    public class UserFilter : Filter<User>
    {
        public long ProfileId;
        public string Username;
        public string Name;
        public string Email;
        public long RoleId;
        public UserType? UserType;
        public DateTime? DateFrom;
        public DateTime? DateTo;

        public override IQueryable<User> BuildQuery(IQueryable<User> query)
        {
            if (UserType.HasValue) query = query.Where(q => q.UserType == UserType);
            if (ProfileId > 0) query = query.Where(q => q.Profile.Id == ProfileId);
            if (RoleId > 0) query = query.Where(q => q.Profile.Id == RoleId);
            if (!string.IsNullOrEmpty(Username)) query = query.Where(q => q.UserName.ToLower().Contains(Username.ToLower()));
            if (!string.IsNullOrEmpty(Name)) query = query.Where(q => q.Name.ToLower().Contains(Name.ToLower()));
            if (!string.IsNullOrEmpty(Email)) query = query.Where(q => q.Email.ToLower().Contains(Email.ToLower()));
            if (DateFrom.HasValue)
            {
                DateFrom = new DateTime(DateFrom.Value.Year, DateFrom.Value.Month, DateFrom.Value.Day, 0, 0, 0);
                query = query.Where(q => q.Created >= DateFrom);
            }
            if (DateTo.HasValue)
            {
                DateTo = new DateTime(DateTo.Value.Year, DateTo.Value.Month, DateTo.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Created <= DateTo);
            }
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class CategoriesFilter : Filter<Category>
    {
        public long Id;
        public long KindId;
        public long TypeId;
        public string Name;

        public override IQueryable<Category> BuildQuery(IQueryable<Category> query)
        {
            query = query.Include(x => x.Type.Kind);
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (TypeId > 0) query = query.Where(q => q.TypeId == TypeId);
            if (KindId > 0) query = query.Where(q => q.Type.KindId == KindId);
            if (!string.IsNullOrEmpty(Name)) query = query.Where(q => q.Name.ToLower().Contains(Name.ToLower()));
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class TypesFilter : Filter<Models.Type>
    {
        public long Id;
        public long KindId;
        public string Name;

        public override IQueryable<Models.Type> BuildQuery(IQueryable<Models.Type> query)
        {
            query = query.Include(x => x.Kind);
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (KindId > 0) query = query.Where(q => q.KindId == KindId);
            if (!string.IsNullOrEmpty(Name)) query = query.Where(q => q.Name.ToLower().Contains(Name.ToLower()));
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class KindsFilter : Filter<Kind>
    {
        public long Id;
        public string Name;

        public override IQueryable<Kind> BuildQuery(IQueryable<Kind> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(Name)) query = query.Where(q => q.Name.ToLower().Contains(Name.ToLower()));
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class QuestionsFilter : Filter<Question>
    {
        public long Id;
        public long CategoryId;
        public string QuestionText;
        public string Batch;

        public override IQueryable<Question> BuildQuery(IQueryable<Question> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (CategoryId > 0) query = query.Where(q => q.CategoryId == CategoryId);
            if (!string.IsNullOrEmpty(QuestionText)) query = query.Where(q => q.QuestionText.ToLower().Contains(QuestionText.ToLower()));
            if (!string.IsNullOrEmpty(Batch)) query = query.Where(q => q.Batch.ToLower().Contains(Batch.ToLower()));
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class ResultsFilter : Filter<Result>
    {
        public long Id;
        public string UserId;
        public long CategoryId;
        public DateTime? DateFrom;
        public DateTime? DateTo;

        public override IQueryable<Result> BuildQuery(IQueryable<Result> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(UserId)) query = query.Where(q => q.UserId.ToLower().Contains(UserId.ToLower()));
            if (CategoryId > 0) query = query.Where(q => q.CategoryIds.Contains(CategoryId.ToString()));
            if (DateFrom.HasValue)
            {
                DateFrom = new DateTime(DateFrom.Value.Year, DateFrom.Value.Month, DateFrom.Value.Day, 0, 0, 0);
                query = query.Where(q => q.Date >= DateFrom);
            }
            if (DateTo.HasValue)
            {
                DateTo = new DateTime(DateTo.Value.Year, DateTo.Value.Month, DateTo.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Date <= DateTo);
            }
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class SupportMessagesFilter : Filter<SupportMessage>
    {
        public long Id;
        public string Message;
        public string Email;
        public string FullName;
        public string PhoneNumber;

        public override IQueryable<SupportMessage> BuildQuery(IQueryable<SupportMessage> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(Message)) query = query.Where(q => q.Message.ToLower().Contains(Message.ToLower()));
            if (!string.IsNullOrEmpty(Email)) query = query.Where(q => q.Email.ToLower().Contains(Email.ToLower()));
            if (!string.IsNullOrEmpty(FullName)) query = query.Where(q => q.FullName.ToLower().Contains(FullName.ToLower()));
            if (!string.IsNullOrEmpty(PhoneNumber)) query = query.Where(q => q.PhoneNumber.ToLower().Contains(PhoneNumber.ToLower()));
            return query;
        }
    }

    public class SubscriptionPlansFilter : Filter<SubscriptionPlan>
    {
        public long Id;
        public string Name;
        public double? AmountFrom;
        public double? AmountTo;

        public override IQueryable<SubscriptionPlan> BuildQuery(IQueryable<SubscriptionPlan> query)
        {
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(Name)) query = query.Where(q => q.Name.ToLower().Contains(Name.ToLower()));
            if (AmountFrom.HasValue) query = query.Where(q => q.Amount >= AmountFrom);
            if (AmountTo.HasValue) query = query.Where(q => q.Amount <= AmountTo);
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class SubscriptionsFilter : Filter<Subscription>
    {
        public long Id;
        public string UserId;
        public long SubscriptionPlanId;
        public DateTime? DateFrom;
        public DateTime? DateTo;
        public SubscriptionStatus? Status;


        public override IQueryable<Subscription> BuildQuery(IQueryable<Subscription> query)
        {
            query = query.Include(x => x.SubscriptionPlan);
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(UserId)) query = query.Where(q => q.UserId.ToLower().Contains(UserId.ToLower()));
            if (SubscriptionPlanId > 0) query = query.Where(q => q.SubscriptionPlanId == SubscriptionPlanId);
            if (DateFrom.HasValue)
            {
                DateFrom = new DateTime(DateFrom.Value.Year, DateFrom.Value.Month, DateFrom.Value.Day, 0, 0, 0);
                query = query.Where(q => q.Date >= DateFrom);
            }
            if (DateTo.HasValue)
            {
                DateTo = new DateTime(DateTo.Value.Year, DateTo.Value.Month, DateTo.Value.Day, 23, 59, 59);
                query = query.Where(q => q.Date <= DateTo);
            }
            if (Status.HasValue) query = query.Where(q => q.Status == Status);
            query = query.Where(x => !x.IsDeleted);
            return query;
        }
    }

    public class SubscriptionPaymentsFilter : Filter<SubscriptionPayment>
    {
        public long Id;
        public string UserId;
        public double? AmountFrom;
        public double? AmountTo;
        public DateTime? DateFrom;
        public DateTime? DateTo;
        public SubscriptionPaymentStatus? Status;

        public override IQueryable<SubscriptionPayment> BuildQuery(IQueryable<SubscriptionPayment> query)
        {
            query = query.Include(x => x.Subscription.User).Include(x=> x.Subscription.SubscriptionPlan);
            if (Id > 0) query = query.Where(q => q.Id == Id);
            if (!string.IsNullOrEmpty(UserId)) query = query.Where(q => q.Subscription.UserId == UserId);
            if (Status.HasValue) query = query.Where(q => q.Status == Status);
            if (AmountFrom.HasValue) query = query.Where(q => q.Amount >= AmountFrom);
            if (AmountTo.HasValue) query = query.Where(q => q.Amount <= AmountTo);
            if (DateFrom.HasValue) query = query.Where(q => q.Date >= DateFrom);
            if (DateTo.HasValue) query = query.Where(q => q.Date <= DateTo);
            return query;
        }
    }

}