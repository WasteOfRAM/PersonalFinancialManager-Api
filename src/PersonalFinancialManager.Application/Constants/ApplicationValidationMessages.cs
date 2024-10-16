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

        public static class AccountTotalMinValue
        {
            public const string Code = "Insufficient funds";
            public const string Description = "Withdraw amount is higher than the current account total.";
        }

        public static class AccountTotalMaxValue
        {
            public const string Code = "Account total limit";
            public const string Description = "Deposit amount will exceed the account total limit. ({0})";
        }

        public static class ForbiddenTransactionDeletion
        {
            public const string Code = "Forbidden deletion";
            public const string Description = "Only latest transaction can be deleted. Try updating the transaction or create a new one with needed corrections.";
        }

        public static class ForbiddenTransactionEdit
        {
            public const string Code = "Forbidden edit";
            public const string Description = "Only latest transaction can be edited. Try creating a new one with needed corrections.";
        }
    }
}