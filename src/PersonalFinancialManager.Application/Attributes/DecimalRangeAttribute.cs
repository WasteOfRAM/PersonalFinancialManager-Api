namespace PersonalFinancialManager.Application.Attributes;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class DecimalRangeAttribute(string minimum, string maximum) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        minimum = minimum.Replace(",", ".");
        maximum = maximum.Replace(",", ".");

        decimal.TryParse(minimum, CultureInfo.InvariantCulture, out decimal minValue);
        decimal.TryParse(maximum, CultureInfo.InvariantCulture, out decimal maxValue);

        if ((decimal)value < minValue || (decimal)value > maxValue) 
        {
            var errorMessage = $"The field {validationContext.DisplayName} must be between {minValue} and {maxValue}.";
            return new ValidationResult(errorMessage, new[] { validationContext.DisplayName });
        }

        return ValidationResult.Success;
    }
}
