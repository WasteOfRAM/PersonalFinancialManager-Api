﻿namespace PersonalFinancialManager.Application.DTOs.Account;

using PersonalFinancialManager.Application.Attributes;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel.DataAnnotations;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;
using static PersonalFinancialManager.Application.Constants.ApplicationCommonConstants;

public record CreateAccountDTO
(
    [property: Required] [property: StringLength(maximumLength: AccountConstants.NameMaxLength)] 
    string Name,

    [property: Required] [property: EnumDataType(typeof(Currency))]
    string Currency,

    [property: Required] [property: EnumDataType(typeof(AccountType))] 
    string AccountType,

    [property: DecimalPrecision(DecimalPrecisionConstant.Precision, DecimalPrecisionConstant.Scale)] [property: DecimalRange(DecimalRangeMinimumValue, DecimalRangeMaximumValue)]
    decimal? Total,

    [property: StringLength(maximumLength: CommonConstants.DescriptionMaxLength, MinimumLength = CommonConstants.DescriptionMinLength)] 
    string? Description
);