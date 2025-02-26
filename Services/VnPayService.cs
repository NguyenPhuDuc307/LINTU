using System.Globalization;
using LMS.Utils;
using LMS.ViewModels.VNPay;
using Microsoft.Extensions.Options;

namespace LMS.Services;

public class VnPayService(IOptions<VnPayConfigOptions> options) : IVnPayService
{
    private readonly VnPayConfigOptions _options = options.Value;

    public string CreatePaymentUrl(HttpContext context, VnPaymentRequest model)
    {
        var tick = DateTime.Now.Ticks.ToString();

        var vnPay = new VnPayLibrary();
        vnPay.AddRequestData("vnp_Version", _options.Version!);
        vnPay.AddRequestData("vnp_Command", _options.Command!);
        vnPay.AddRequestData("vnp_TmnCode", _options.TmnCode!);
        vnPay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString(CultureInfo.InvariantCulture));

        vnPay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
        vnPay.AddRequestData("vnp_CurrCode", _options.CurrCode!);
        vnPay.AddRequestData("vnp_IpAddr", UtilityHelper.GetIpAddress(context));
        vnPay.AddRequestData("vnp_Locale", _options.Locale!);

        vnPay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng: " + model.OrderId);
        vnPay.AddRequestData("vnp_OrderType", "other");
        vnPay.AddRequestData("vnp_ReturnUrl", _options.PaymentBackReturnUrl!);

        vnPay.AddRequestData("vnp_TxnRef", tick);

        var paymentUrl = vnPay.CreateRequestUrl(_options.BaseUrl!, _options.HashSecret!);

        return paymentUrl;
    }

    public VnPaymentResponse PaymentExecute(IQueryCollection collections)
    {
        var vnPay = new VnPayLibrary();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnPay.AddResponseData(key, value.ToString());
            }
        }

        var vnpOrderId = Convert.ToInt64(vnPay.GetResponseData("vnp_TxnRef"));
        var vnpTransactionId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
        var vnpSecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
        var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
        var vnpOrderInfo = vnPay.GetResponseData("vnp_OrderInfo");
        var vnpAmount = decimal.Parse(vnPay.GetResponseData("vnp_Amount"));

        var checkSignature = vnPay.ValidateSignature(vnpSecureHash!, _options.HashSecret!);
        if (!checkSignature)
        {
            return new VnPaymentResponse
            {
                Success = false
            };
        }

        return new VnPaymentResponse
        {
            Amount =  vnpAmount,
            Success = true,
            PaymentMethod = "VnPay",
            OrderDescription = vnpOrderInfo,
            OrderId = vnpOrderId.ToString(),
            TransactionId = vnpTransactionId.ToString(),
            Token = vnpSecureHash,
            VnPayResponseCode = vnpResponseCode
        };
    }
}