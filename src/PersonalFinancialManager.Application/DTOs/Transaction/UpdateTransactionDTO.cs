﻿namespace PersonalFinancialManager.Application.DTOs.Transaction;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;
using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public record UpdateTransactionDTO
(
    [property: Required] [property: GuidDataType]
    string Id,

    [property: Required] [property: GuidDataType]
    string AccountId,

    [property : Required] [property: EnumDataType(typeof(TransactionType))]
    string TransactionType,

    [property: DecimalPrecision(DecimalPrecisionConstant.Precision, DecimalPrecisionConstant.Scale)] [property: Range(0.0, 999999999999999.9999)]
    decimal Amount,

    [property : StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    string? Description
);