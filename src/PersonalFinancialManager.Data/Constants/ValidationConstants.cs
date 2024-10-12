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
        public const int Precision = 19;
        public const int Scale = 4;
    }

    public static class DecimalRangeConstants
    {
        public const string DecimalRangeMinimumValue = "0.0";
        public const string DecimalRangeMaximumValue = "999999999999999.9999";
    }

    public static class AccountConstants
    {
        public const int NameMaxLength = 10;

        public const int CurrencyMaxLength = 5;
    }
}
