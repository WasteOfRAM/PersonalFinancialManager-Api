namespace PersonalFinancialManager.Core.Entities;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }

    public TransactionType TransactionType { get; set; }

    [DefaultValue(CommonConstants.MoneyDefaultValue)]
    [Precision(DecimalPrecisionConstant.Integer, DecimalPrecisionConstant.Fraction)]
    public decimal Amount { get; set; }

    public DateTime CreationDate { get; set; }

    [StringLength(maximumLength: CommonConstants.DescriptionMaxLength)]
    public string? Description { get; set; }

    public Guid AccountId { get; set; }
    [ForeignKey(nameof(AccountId))]
    public virtual Account? Account { get; set; }
}