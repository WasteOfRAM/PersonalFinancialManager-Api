﻿namespace PersonalFinancialManager.Application.DTOs.Transaction;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;
using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public record CreateTransactionDTO
(
    [property: Required] [property: GuidDataType]
    string AccountId,

    [property : Required] [property : EnumDataType(typeof(TransactionType))]
    string TransactionType,

    [property: DecimalPrecision(DecimalPrecisionConstant.Precision, DecimalPrecisionConstant.Scale)] [property: Range(0.0, double.PositiveInfinity)]
    decimal Amount,

    [property : StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)]
    string? Description
);