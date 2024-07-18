namespace PersonalFinancialManager.Core.Entities;

using Microsoft.EntityFrameworkCore;
using PersonalFinancialManager.Core.Enumerations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }

    public TransactionType Type { get; set; }

    [DefaultValue(0.0)]
    [Precision(19, 4)]
    public decimal Amount { get; set; }

    public DateTime CreationDate { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }
}