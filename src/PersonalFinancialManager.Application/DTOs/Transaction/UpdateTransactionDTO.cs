namespace PersonalFinancialManager.Application.DTOs.Transaction;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;
using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;

public record UpdateTransactionDTO
(
    [property: Required] [property: GuidDataType]
    string Id,

    [property: Required] [property: GuidDataType]
    string AccountId,

    [property : Required] [property: EnumDataType(typeof(TransactionType))]
    string TransactionType,

    [property: DecimalPrecision(DecimalPrecisionConstant.Precision, DecimalPrecisionConstant.Scale)] [property: DecimalRange(DecimalRangeMinimumValue, DecimalRangeMaximumValue)]
    decimal Amount,

    [property : StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    string? Description
);