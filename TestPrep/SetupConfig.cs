namespace TestPrep
{
    public static class SetupConfig
    {
        public static Setting Setting { get; set; }
    }

    public class Setting
    {
        public long ServiceTimer { get; set; }
        public int GracePeriod { get; set; }
        public double BasePaymentFee { get; set; }
        public PasswordPolicies PasswordPolicies { get; set; }
        public string RandomCharacters { get; set; }
    }

    public class PasswordPolicies
    {
        public int MinimumLength { get; set; }
        public string SpecialCharacters { get; set; }
        public int PreviousPasswordCount { get; set; }
        public bool RequireNonLetterOrDigit { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
    }
}