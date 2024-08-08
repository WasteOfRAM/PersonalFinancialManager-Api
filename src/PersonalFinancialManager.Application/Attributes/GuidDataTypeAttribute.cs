namespace PersonalFinancialManager.Application.Attributes;

using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
sealed public class GuidDataTypeAttribute : ValidationAttribute
{
    public GuidDataTypeAttribute() : base("'{0}' is not a valid uuid value.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (!Guid.TryParse((string)value!, result: out _))
        {
            var errorMessage = FormatErrorMessage(validationContext.DisplayName);
            return new ValidationResult(errorMessage, new[] { validationContext.DisplayName });
        }

        return ValidationResult.Success;
    }
}
