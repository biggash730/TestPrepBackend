using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace TestPrep.IdentityExtensions
{
    public class CustomPasswordValidator : IIdentityValidator<string>
    {
        public int MinimumLength { get; set; }
        public string SpecialCharacters { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public int PreviousPasswordCount { get; set; }

        public CustomPasswordValidator()
        {
            MinimumLength = SetupConfig.Setting.PasswordPolicies.MinimumLength;
            SpecialCharacters = SetupConfig.Setting.PasswordPolicies.SpecialCharacters;
            RequireDigit = SetupConfig.Setting.PasswordPolicies.RequireDigit;
            RequireLowercase = SetupConfig.Setting.PasswordPolicies.RequireLowercase;
            RequireUppercase = SetupConfig.Setting.PasswordPolicies.RequireUppercase;
            PreviousPasswordCount = SetupConfig.Setting.PasswordPolicies.PreviousPasswordCount;
        }

        public Task<IdentityResult> ValidateAsync(string item)
        {
            if (string.IsNullOrEmpty(item) || item.Length < MinimumLength)
            {
                return
                    Task.FromResult(
                        IdentityResult.Failed($"Password should be a minimum length of {MinimumLength} characters"));
            }

            if (string.IsNullOrEmpty(item) || RequireLowercase)
            {
                if(!item.Any(char.IsLower))
                return
                    Task.FromResult(
                        IdentityResult.Failed("Password should have at least one lower case character"));
            }
            if (string.IsNullOrEmpty(item) || RequireUppercase)
            {
                if (!item.Any(char.IsUpper))
                    return
                        Task.FromResult(
                            IdentityResult.Failed("Password should have at least one upper case character"));
            }

            if (string.IsNullOrEmpty(item) || RequireDigit)
            {
                if (!item.Any(char.IsDigit))
                    return
                        Task.FromResult(
                            IdentityResult.Failed("Password should have at least one number"));
            }
            if (!string.IsNullOrEmpty(item) && SpecialCharacters.Length <= 0)
                return Task.FromResult(IdentityResult.Success);
            var reg = new Regex(@"[" +SpecialCharacters+ "]+");
            return Task.FromResult(!reg.IsMatch(item) ? IdentityResult.Failed($"Password should have at least one of these special characters - {SpecialCharacters.Replace("|", ",")}") : IdentityResult.Success);
        }
    }
}