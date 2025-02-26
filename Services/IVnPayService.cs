using LMS.ViewModels.VNPay;

namespace LMS.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(HttpContext context, VnPaymentRequest model);
    VnPaymentResponse PaymentExecute(IQueryCollection collections);
}