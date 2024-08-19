namespace PersonalFinancialManager.Application.Constants;

public static class ApplicationValidationMessages
{
    public static class ErrorMessages
    {
        public static class DuplicateName
        {
            public const string Code = "DuplicateName";
            public const string Description = "Name '{0}' already exists.";
        }

        public static class AccountId
        {
            public const string Code = "AccountId";
            public const string Description = "Account with given Id does not exist.";
        }
    }
}