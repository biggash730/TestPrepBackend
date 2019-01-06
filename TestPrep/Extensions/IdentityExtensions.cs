using System.Security.Principal;
using System.Threading.Tasks;
using TestPrep.DataAccess.Repositories;
using TestPrep.Models;

namespace TestPrep.Extensions
{
    public static class IdentityExtensions
    {
        public static async Task<User> AsAppUser(this IIdentity identity)
        {
            var user = new UserRepository().Get(identity.Name);
            return await Task.FromResult(user);
        }
    }
}