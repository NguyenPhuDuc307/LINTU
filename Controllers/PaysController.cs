using LMS.Data;
using LMS.Data.Entities;
using LMS.Data.Entities.Enums;
using LMS.Models;
using LMS.Services;
using LMS.ViewModels.VNPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để thanh toán
    public class PaysController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PaysController(IVnPayService vnPayService, ApplicationDbContext context, UserManager<User> userManager)
        {
            _vnPayService = vnPayService;
            _context = context;
            _userManager = userManager;
        }

        public static Dictionary<string, string> vnp_TransactionStatus = new Dictionary<string, string>()
        {
            {"00","Giao dịch thành công" },
            {"01","Giao dịch chưa hoàn tất" },
            {"02","Giao dịch bị lỗi" },
            {"04","Giao dịch đảo (Khách hàng đã bị trừ tiền tại Ngân hàng nhưng GD chưa thành công ở VNPAY)" },
            {"05","VNPAY đang xử lý giao dịch này (GD hoàn tiền)" },
            {"06","VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng (GD hoàn tiền)" },
            {"07","Giao dịch bị nghi ngờ gian lận" },
            {"09","GD Hoàn trả bị từ chối" }
        };

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Pay()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Pay(CheckoutViewModel request)
        {
            if (ModelState.IsValid)
            {
                if (request.PaymentMethod == "VNPay")
                {
                    string cleanedAmount = request.Amount!.Replace(".", "");
                    var vnPayModel = new VnPaymentRequest
                    {
                        Amount = Double.Parse(cleanedAmount),
                        CreatedDate = DateTime.Now,
                        Description = $"{request.FullName} {request.PhoneNumber}",
                        FullName = request.FullName,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }

                return View();
            }
            return View(request);
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        // Callback xử lý kết quả thanh toán từ VNPay
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00") // Thanh toán thành công
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Message"] = "User not found.";
                    return RedirectToAction(nameof(PaymentFail));
                }

                decimal amount = response.Amount / 100m; // Chia 100 vì VNPay trả về đơn vị VNĐ nhỏ hơn thực tế

                // Kiểm tra xem giao dịch này đã được xử lý chưa (tránh double spending)
                bool transactionExists = await _context.Transactions.AnyAsync(t =>
                    t.UserId == user.Id && t.Amount == amount && t.TransactionType == TransactionType.Deposit);

                if (!transactionExists)
                {
                    // Tạo giao dịch mới
                    var transaction = new Transaction
                    {
                        UserId = user.Id,
                        Amount = amount,
                        TransactionType = TransactionType.Deposit
                    };

                    _context.Transactions.Add(transaction);
                    await _context.SaveChangesAsync();

                    TempData["Message"] = $"Payment successful! Amount: {amount:C}";
                    return RedirectToAction(nameof(PaymentSuccess));
                }
                else
                {
                    TempData["Message"] = "This transaction has already been processed.";
                    return RedirectToAction(nameof(PaymentFail));
                }
            }

            // Nếu thất bại, lấy thông tin lỗi từ dictionary
            if (vnp_TransactionStatus.TryGetValue(response.VnPayResponseCode!, out var message))
            {
                TempData["Message"] = $"Payment error: {message}";
            }
            else
            {
                TempData["Message"] = $"Unknown payment error: {response.VnPayResponseCode}";
            }

            return RedirectToAction(nameof(PaymentFail));
        }
    }
}
