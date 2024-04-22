using FluentValidation;

namespace IP.Project.Extensions
{
    public static class ValidationExtensions
    {

        public static IRuleBuilderOptions<T, string> Matricol<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[0-9]{9}[A-Z]{3}[0-9]{6}$").WithMessage("Matricol is not valid");
        }
    }
}
