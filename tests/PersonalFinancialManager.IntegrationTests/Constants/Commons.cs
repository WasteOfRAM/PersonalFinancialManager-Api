namespace PersonalFinancialManager.IntegrationTests.Constants;

public static class Commons
{
    public static class UserEmails
    {
        public const string TestUserEmail = "user@test.com";
    }

    public static class Passwords
    {
        public const string ApplicationValidPassword = "Te123456789$";
        public const string ApplicationValidPasswordTwo = "aG987654321*";

        public const string ApplicationInvalidPasswordOne = "Te1234$";
        public const string ApplicationInvalidPasswordTwo = "Te123456789";
        public const string ApplicationInvalidPasswordThree = "te123456789$";
    }
}
