using System.Linq;
using LendingSquareAPI.Models;

namespace LendingSquareAPI.DataAccess.Repositories
{
    public class AppSettingRepository : BaseRepository<AppSetting>
    {
        public AppSetting Get(string name)
        {
            return DbSet.FirstOrDefault(q => q.Name == name);
        }
    }
}