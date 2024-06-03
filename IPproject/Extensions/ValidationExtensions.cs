using FluentValidation;

namespace IP.Project.Extensions
{
    public static class ValidationExtensions
    {

        public static IRuleBuilderOptions<T, string> Matricol<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[0-9]{9}[A-Z]{3}[0-9]{6}$").WithMessage("Matricol is not valid");
        }
        public static IRuleBuilderOptions<T, string> CNP<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[1-9]{1}[0-9]{12}$").WithMessage("CNP is not valid");
        }
        public static IRuleBuilderOptions<T, string> IpAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
                                .WithMessage("Invalid IP address format");
        }
        public static IRuleBuilderOptions<T, string> UniversityEmailAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[a-zA-Z0-9.]+@info.uaic.ro$").WithMessage("Invalid university email address");
        }   
        public static IRuleBuilderOptions<T, int> UidNumber<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.InclusiveBetween(1199, 7999).WithMessage("UID number is not valid");
        }
        public static IRuleBuilderOptions<T, string> OnlyNumbers<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^[0-9]*$").WithMessage("Telephone number is not valid");
        }
        
        public static IRuleBuilderOptions<T, string> TrimmedMinLength<T>(this IRuleBuilder<T, string> ruleBuilder, int min)
        {
            return ruleBuilder.Must(x => x?.Trim().Length > min);
        }
        
        public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
                .WithMessage("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit and one special character.");
        }
    }
}
