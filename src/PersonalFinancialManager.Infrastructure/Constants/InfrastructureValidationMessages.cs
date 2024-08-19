namespace PersonalFinancialManager.Infrastructure.Constants;

public static class InfrastructureValidationMessages
{
    public static class ErrorMessages
    {
        public static class InvalidLogin
        {
            public const string Code = "invalid login";
            public const string Description = "Invalid email or password.";
        }
    }
}