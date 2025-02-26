using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels.VNPay;

public class VnPaymentRequest
{
    public int OrderId { get; set; }
    public string? FullName { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please enter the amount")]
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; }
}