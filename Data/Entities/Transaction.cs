using System.ComponentModel.DataAnnotations.Schema;
using LMS.Data.Entities.Enums;

namespace LMS.Data.Entities;

public class Transaction : IDateTracking
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public User? User { get; set; }
    [Column(TypeName = "decimal(18,0)")]
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }

    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}