namespace PersonalFinancialManager.Core.Entities;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static PersonalFinancialManager.Core.Constants.ValidationConstants;

public class Account
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(maximumLength: AccountConstants.NameMaxLength)]
    public required string Name { get; set; }

    [StringLength(maximumLength: AccountConstants.CurrencyMaxLength)]
    public required string Currency { get; set; }

    public AccountType AccountType { get; set; }

    public DateTime CreationDate { get; set; }

    [DefaultValue(CommonConstants.MoneyDefaultValue)]
    [Precision(DecimalPrecisionConstant.Precision, DecimalPrecisionConstant.Scale)]
    public decimal Total { get; set; }

    [StringLength(maximumLength: CommonConstants.DescriptionMaxLength)]
    public string? Description { get; set; }

    public Guid AppUserId { get; set; }
    [ForeignKey(nameof(AppUserId))]
    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<Transaction>? Transactions { get; set; }
}