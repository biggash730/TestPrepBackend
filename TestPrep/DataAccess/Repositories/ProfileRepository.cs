using System.Linq;
using TestPrep.Models;

namespace TestPrep.DataAccess.Repositories
{
    public class ProfileRepository : BaseRepository<Profile>
    {
        public override void Update(Profile entity)
        {
            var theProfile = DbSet.Find(entity.Id);
            theProfile.Name = entity.Name;
            theProfile.Privileges = entity.Privileges;
            theProfile.Notes = entity.Notes;
            SaveChanges();
        }

        public Profile GetProfileByName(string name)
        {
            return DbSet.FirstOrDefault(x => x.Name == name);
        }

        public Profile GetProfileByName(string name, AppDbContext db)
        {
            return db.Profiles.FirstOrDefault(x => x.Name == name);
        }
    }
}