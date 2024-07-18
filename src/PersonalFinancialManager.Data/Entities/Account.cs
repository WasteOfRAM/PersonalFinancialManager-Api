namespace PersonalFinancialManager.Core.Entities;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Account
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(10)]
    public string Name { get; set; } = null!;

    [MaxLength(5)]
    public string Currency { get; set; } = null!;

    public AccountType Type { get; set; }

    public DateTime CreationDate { get; set; }

    [DefaultValue(0.0)]
    [Precision(19, 4)]
    public decimal Total { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    public virtual ICollection<Transaction>? Transactions { get; set; }
}