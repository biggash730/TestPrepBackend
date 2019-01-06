using System.Linq;
using TestPrep.Models;

namespace TestPrep.DataAccess.Repositories
{
    public class VatNhilTransactionRepository : BaseRepository<VatNhilTransaction>
    {
        public VatNhilTransaction GetTransaction(long id)
        {
            return DbSet.FirstOrDefault(x => x.Id == id);
        }

        public VatNhilTransaction GetTransactionByInvoiceNumber(string invoiceNumber)
        {
            return DbSet.FirstOrDefault(x => x.InvoiceNumber == invoiceNumber);
        }

    }
}