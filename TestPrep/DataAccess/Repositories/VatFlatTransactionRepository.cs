using System.Linq;
using TestPrep.Models;

namespace TestPrep.DataAccess.Repositories
{
    public class VatFlatTransactionRepository : BaseRepository<VatFlatTransaction>
    {
        public VatFlatTransaction GetTransaction(long id)
        {
            return DbSet.FirstOrDefault(x => x.Id == id);
        }

        public VatFlatTransaction GetTransactionByInvoiceNumber(string invoiceNumber)
        {
            return DbSet.FirstOrDefault(x => x.InvoiceNumber == invoiceNumber);
        }

    }
}