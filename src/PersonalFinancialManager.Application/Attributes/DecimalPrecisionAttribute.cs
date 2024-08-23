namespace PersonalFinancialManager.Application.Attributes;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class DecimalPrecisionAttribute(int precision, int scale) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is decimal decimalValue)
        {
            var parts = decimalValue.ToString(CultureInfo.InvariantCulture).Split('.');
            var integerPartLength = parts[0].Length;
            var fractionalPartLength = parts.Length > 1 ? parts[1].Length : 0;

            if (integerPartLength > precision - scale || fractionalPartLength > scale)
            {
                var errorMessage = $"The field {validationContext.DisplayName} must have a precision of {precision} digits and a scale of {scale} digits.";
                return new ValidationResult(errorMessage, new[] { validationContext.DisplayName });
            }
        }

        return ValidationResult.Success;
    }
}
