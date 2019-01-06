using System;
using System.Globalization;
using System.Linq;
using TestPrep.AxHelpers;
using TestPrep.Models;

namespace TestPrep.DataAccess.Repositories
{
    public class VatFlatPaymentRepository : BaseRepository<VatFlatPayment>
    {
        public VatFlatPayment Generate(int month, int year, User user)
        {
            var rec = new VatFlatPayment();

            //check if there is a payment record like that
            var exisiting = DbContext.VatFlatPayments.FirstOrDefault(x => x.Month == month && x.Year == year && x.UserId == user.Id);
            if (exisiting == null || exisiting.Status == PaymentStatus.Cancelled)
            {
                var vfp = new VatFlatPayment
                {
                    Date = DateTime.Now,
                    Fee = SetupConfig.Setting.BasePaymentFee,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    CreatedBy = user.UserName,
                    ModifiedBy = user.UserName,
                    Month = month,
                    Year = year,
                    Status = PaymentStatus.Generated,
                    AmountPayable = 0,
                    UserId = user.Id
                };
                var recs = DbContext.VatFlatTransactions.Where(x => x.Date.Month == month & x.Date.Year == year && x.UserId == user.Id);
                if (recs.Any()) vfp.AmountPayable = recs.Sum(x => x.Tax);

                rec = vfp;
            }
            else
            {
                switch (exisiting.Status)
                {
                    case PaymentStatus.Generated:
                        throw new Exception("There is already a generated payment record for the selected period. Please Confirm it and Initiate Payment.");
                    case PaymentStatus.Unpaid:
                        throw new Exception("There is already an unpaid payment record for the selected period. Please Initiate Payment.");
                    case PaymentStatus.Paid:
                        throw new Exception("There is already a paid payment record for the selected period.");
                    case PaymentStatus.Failed:
                        throw new Exception("There is already a payment record for the selected period with a failed status. Please retry the payment Initiation");
                }
            }
            DbContext.VatFlatPayments.Add(rec);
            SaveChanges();
            return rec;
        }

        public void Confirm(long id, User user)
        {
            var exisiting = DbContext.VatFlatPayments.FirstOrDefault(x => x.Id == id && x.UserId == user.Id);
            if (exisiting == null) throw new Exception("Please check the Id provided.");
            exisiting.ModifiedAt = DateTime.Now;
            exisiting.ModifiedBy = user.UserName;
            exisiting.Status = PaymentStatus.Unpaid;
            SaveChanges();

            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(exisiting.Month);

            var newSt = new VatFlatStatement
            {
                Date = DateTime.Now,
                Amount = exisiting.AmountPayable,
                Description = $"Confirmed VAT Flat Rate Amount for the period: {mnth}-{exisiting.Year}",
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Debit
            };

            var st = DbContext.VatFlatStatements.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Date).FirstOrDefault();
            if (st != null) newSt.Balance = st.Balance - exisiting.AmountPayable;
            else newSt.Balance = 0 - exisiting.AmountPayable;

            DbContext.VatFlatStatements.Add(newSt);
            SaveChanges();
        }

        public void Cancel(long id, User user)
        {
            var exisiting = DbContext.VatFlatPayments.FirstOrDefault(x => x.Id == id && x.UserId == user.Id);
            if (exisiting == null) throw new Exception("Please check the Id provided.");
            var oldStatus = exisiting.Status;
            exisiting.ModifiedAt = DateTime.Now;
            exisiting.ModifiedBy = user.UserName;
            exisiting.Status = PaymentStatus.Cancelled;
            SaveChanges();
            if(oldStatus != PaymentStatus.Unpaid) return;
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(exisiting.Month);

            var newSt = new VatFlatStatement
            {
                Date = DateTime.Now,
                Amount = exisiting.AmountPayable,
                Description = $"Cancelled VAT Flat Rate Amount for the period: {mnth}-{exisiting.Year}",
                Balance = exisiting.AmountPayable,
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Credit
            };

            var st = DbContext.VatFlatStatements.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Date).First();
            newSt.Balance = st.Balance + exisiting.AmountPayable;
            DbContext.VatFlatStatements.Add(newSt);
            SaveChanges();
        }

        public void MakeMomoPayment(VatFlatMomoPayment record, User user)
        {
            record.Status =MobileMoneyTransactionStatus.Pending;
            record.Date = DateTime.Now;
            DbContext.VatFlatMomoPayments.Add(record);
            SaveChanges();
            
            var pymnt = DbContext.VatFlatPayments.First(x => x.Id == record.VatFlatPaymentId);
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pymnt.Month);
            var newSt = new VatFlatStatement
            {
                Date = DateTime.Now,
                Amount = pymnt.AmountPayable,
                Description = $"Payment for VAT Flat Rate for the period: {mnth}-{pymnt.Year}",
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Credit
            };
            var st = DbContext.VatFlatStatements.Where(x => x.Id > 0).OrderByDescending(x => x.Date).First();
            newSt.Balance = st.Balance + pymnt.AmountPayable;
            DbContext.VatFlatStatements.Add(newSt);
            pymnt.Status = PaymentStatus.Paid;
            SaveChanges();

            // Send E receipt
            var msg = new Message
            {
                Text =
                    $"Hello {user.Name}, Your VAT Flat Rate Payment for {mnth} {pymnt.Year} has been received. Amount: GHS {pymnt.AmountPayable}, Reference: {record.Reference}, Payment Mode: Mobile Money. Thank you.",
                Subject = "Processed Payment",
                Recipient = user.PhoneNumber,
                TimeStamp = DateTime.Now,
                Status = MessageStatus.Pending
            };
            DbContext.Messages.Add(msg);
            SaveChanges();
        }

        public void MakeCardPayment(VatFlatCardPayment record, User user)
        {
            record.Status = BankCardPaymentStatus.Pending;
            record.Date = DateTime.Now;
            DbContext.VatFlatCardPayments.Add(record);
            SaveChanges();

            var pymnt = DbContext.VatFlatPayments.First(x => x.Id == record.VatFlatPaymentId);
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pymnt.Month);
            var newSt = new VatFlatStatement
            {
                Date = DateTime.Now,
                Amount = pymnt.AmountPayable,
                Description = $"Payment for VAT Flat Rate for the period: {mnth}-{pymnt.Year}",
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Credit
            };
            var st = DbContext.VatFlatStatements.Where(x => x.Id > 0).OrderByDescending(x => x.Date).First();
            newSt.Balance = st.Balance + pymnt.AmountPayable;
            DbContext.VatFlatStatements.Add(newSt);
            pymnt.Status = PaymentStatus.Paid;
            SaveChanges();

            // Send E receipt
            var msg = new Message
            {
                Text =
                    $"Hello {user.Name}, Your VAT Flat Rate Payment for {mnth} {pymnt.Year} has been received. Amount: GHS {pymnt.AmountPayable}, Reference: {record.Reference}, Payment Mode: Bank Card. Thank you.",
                Subject = "Processed Payment",
                Recipient = user.PhoneNumber,
                TimeStamp = DateTime.Now,
                Status = MessageStatus.Pending
            };
            DbContext.Messages.Add(msg);
            SaveChanges();
        }
    }
}