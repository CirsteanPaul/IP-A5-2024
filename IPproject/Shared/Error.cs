namespace IP.Project.Shared
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "Value cannot be null.");
        public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "Condition not met.");
    }
}
