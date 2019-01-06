using System;
using System.Globalization;
using System.Linq;
using TestPrep.AxHelpers;
using TestPrep.Models;

namespace TestPrep.DataAccess.Repositories
{
    public class VatNhilPaymentRepository : BaseRepository<VatNhilPayment>
    {
        public VatNhilPayment Generate(int month, int year, User user)
        {
            var rec = new VatNhilPayment();

            //check if there is a payment record like that
            var exisiting = DbContext.VatNhilPayments.FirstOrDefault(x => x.Month == month && x.Year == year && x.UserId == user.Id);
            if (exisiting == null || exisiting.Status == PaymentStatus.Cancelled)
            {
                var vnp = new VatNhilPayment
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
                    InputAmount = 0,
                    OutputAmount = 0,
                    UserId = user.Id
                };
                var recs = DbContext.VatNhilTransactions.Where(x => x.Date.Month == month & x.Date.Year == year && x.UserId == user.Id);
                if (recs.Any())
                {


                    var totalInputs = recs.Where(x => x.Type == TransactionType.Input).Sum(x => x.Tax);
                    var totalOutputs = recs.Where(x => x.Type == TransactionType.Output).Sum(x => x.Tax);
                    vnp.InputAmount = totalInputs;
                    vnp.OutputAmount = totalOutputs;
                    vnp.AmountPayable = totalOutputs - totalInputs;
                }
                rec = vnp;
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
            DbContext.VatNhilPayments.Add(rec);
            SaveChanges();
            return rec;
        }

        public void Confirm(long id, User user)
        {
            var exisiting = DbContext.VatNhilPayments.FirstOrDefault(x => x.Id == id && x.UserId == user.Id);
            if (exisiting == null) throw new Exception("Please check the Id provided.");
            exisiting.ModifiedAt = DateTime.Now;
            exisiting.ModifiedBy = user.UserName;
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(exisiting.Month);
            var newSt = new VatNhilStatement
            {
                Date = DateTime.Now,
                Amount = exisiting.AmountPayable,
                Description = $"Confirmed VAT + NHIL amount for the period: {mnth}-{exisiting.Year}",
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Debit
            };

            var st = DbContext.VatNhilStatements.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Date).FirstOrDefault();
            if (st != null) newSt.Balance = st.Balance - exisiting.AmountPayable;
            else newSt.Balance = 0 - exisiting.AmountPayable;

            exisiting.Status = exisiting.AmountPayable <= 0 ? PaymentStatus.Paid : PaymentStatus.Unpaid;
            
            DbContext.VatNhilStatements.Add(newSt);
            SaveChanges();
        }

        public void Cancel(long id, User user)
        {
            var exisiting = DbContext.VatNhilPayments.FirstOrDefault(x => x.Id == id && x.UserId == user.Id);
            if (exisiting == null) throw new Exception("Please check the Id provided.");
            exisiting.ModifiedAt = DateTime.Now;
            exisiting.ModifiedBy = user.UserName;
            exisiting.Status = PaymentStatus.Cancelled;
            SaveChanges();
            var oldStatus = exisiting.Status;
            if (oldStatus != PaymentStatus.Unpaid) return;
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(exisiting.Month);
            var newSt = new VatNhilStatement
            {
                Date = DateTime.Now,
                Amount = exisiting.AmountPayable,
                Description = $"Cancelled VAT + NHIL Rate amount for the period: {mnth}-{exisiting.Year}",
                //Balance = exisiting.AmountPayable,
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Credit
            };

            var st = DbContext.VatNhilStatements.Where(x => x.UserId == user.Id).OrderByDescending(x => x.Date).First();
            newSt.Balance = st.Balance + exisiting.AmountPayable;
            DbContext.VatNhilStatements.Add(newSt);
            SaveChanges();
        }

        public void MakeMomoPayment(VatNhilMomoPayment record, User user)
        {
            record.Status = MobileMoneyTransactionStatus.Pending;
            record.Date = DateTime.Now;
            DbContext.VatNhilMomoPayments.Add(record);
            SaveChanges();
            var pymnt = DbContext.VatNhilPayments.First(x => x.Id == record.VatNhilPaymentId);
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pymnt.Month);
            var newSt = new VatNhilStatement
            {
                Date = DateTime.Now,
                Amount = pymnt.AmountPayable,
                Description = $"Payment for VAT + NHIL Rate for the period: {mnth}-{pymnt.Year}",
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Credit
            };
            var st = DbContext.VatNhilStatements.Where(x => x.Id > 0).OrderByDescending(x => x.Date).First();
            newSt.Balance = st.Balance + pymnt.AmountPayable;
            DbContext.VatNhilStatements.Add(newSt);
            pymnt.Status = PaymentStatus.Paid;
            SaveChanges();

            // Send E receipt
            
            var msg = new Message
            {
                Text =
                    $"Hello {user.Name}, Your VAT+NHIL Rate Payment for {mnth} {pymnt.Year} has been received. Amount: GHS {pymnt.AmountPayable}, Reference: {record.Reference}, Payment Mode: Mobile Money. Thank you.",
                Subject = "Processed Payment",
                Recipient = user.PhoneNumber,
                TimeStamp = DateTime.Now,
                Status = MessageStatus.Pending
            };
            DbContext.Messages.Add(msg);
            SaveChanges();
        }

        public void MakeCardPayment(VatNhilCardPayment record, User user)
        {
            record.Status = BankCardPaymentStatus.Pending;
            record.Date = DateTime.Now;
            DbContext.VatNhilCardPayments.Add(record);
            SaveChanges();

            var pymnt = DbContext.VatNhilPayments.First(x => x.Id == record.VatNhilPaymentId);
            var mnth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(pymnt.Month);
            var newSt = new VatNhilStatement
            {
                Date = DateTime.Now,
                Amount = pymnt.AmountPayable,
                Description = $"Payment for VAT + NHIL Rate for the period: {mnth}-{pymnt.Year}",
                UserId = user.Id,
                StatementEntryType = StatementEntryType.Credit
            };
            var st = DbContext.VatNhilStatements.Where(x => x.Id > 0).OrderByDescending(x => x.Date).First();
            newSt.Balance = st.Balance + pymnt.AmountPayable;
            DbContext.VatNhilStatements.Add(newSt);
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