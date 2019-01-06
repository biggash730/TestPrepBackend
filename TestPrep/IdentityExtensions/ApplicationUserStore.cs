using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using TestPrep.Models;

namespace TestPrep.IdentityExtensions
{
    public class ApplicationUserStore : UserStore<User>
    {
        public ApplicationUserStore(AppDbContext context)
        : base(context)
        {
        }
        public override async Task CreateAsync(User user)
        {
            await base.CreateAsync(user);
            
            await AddToPreviousPasswordsAsync(user, user.PasswordHash);
        }
        public Task AddToPreviousPasswordsAsync(User user, string password)
        {
            var obj = new PreviousPassword {UserId = user.Id, PasswordHash = password};
            //todo: check the implementation
            //user.PreviousUserPasswords.Add(obj);
            return UpdateAsync(user);
        }
    }

}