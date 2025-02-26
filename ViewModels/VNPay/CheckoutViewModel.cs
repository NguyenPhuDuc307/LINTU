using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels.VNPay;

public class CheckoutViewModel
{
    public string? PaymentMethod { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    [Required]
    public string? Amount { get; set; }
    public string? Note { get; set; }
}