using FluentValidation;

namespace IP.Project.Extensions
{
    public static class ValidationExtensions
    {

        public static IRuleBuilderOptions<T, string> Matricol<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[0-9]{9}[A-Z]{3}[0-9]{6}$").WithMessage("Matricol is not valid");
        }
        public static IRuleBuilderOptions<T, string> IpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
                                .WithMessage("Invalid IP address format");
        }
    }
}
