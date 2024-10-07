namespace PersonalFinancialManager.IntegrationTests.Constants;

public static class EndpointsV1
{
    public static class UserEndpoints
    {
        public const string Register = "/api/v1/user/register";
        public const string Login = "/api/v1/user/login";
        public const string Refresh = "/api/v1/user/refresh";
    }

    public static class AccountEndpoints
    {
        public const string AccountBase = "/api/v1/account";
        public const string AccountWithId = "/api/v1/account/{0}";
        public const string AccountWithIdAndTransactions = "/api/v1/account/{0}/transactions";
    }
}
