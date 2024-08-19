namespace PersonalFinancialManager.Core.Constants;

public static class ValidationConstants
{
    public static class CommonConstants
    {
        public const int DescriptionMaxLength = 100;
        public const int DescriptionMinLength = 2;

        public const double MoneyDefaultValue = 0.0;
    }

    public static class DecimalPrecisionConstant
    {
        public const int Integer = 19;
        public const int Fraction = 4;
    }

    public static class AccountConstants
    {
        public const int NameMaxLength = 10;

        public const int CurrencyMaxLength = 5;
    }
}
