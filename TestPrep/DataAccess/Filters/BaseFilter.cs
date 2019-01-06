using System.Linq;

namespace TestPrep.DataAccess.Filters
{
    public abstract class Filter<T>
    {
        public abstract IQueryable<T> BuildQuery(IQueryable<T> query);
        public Pager Pager = new Pager();
    }

    public abstract class ReportFilter<T>
    {
        public abstract IQueryable<T> BuildQuery(IQueryable<T> query);
    }

    public class Pager
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public int Skip(){return (Page - 1) * Size;}
    }
}