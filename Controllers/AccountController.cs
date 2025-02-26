using LMS.Data;
using LMS.Data.Entities;
using LMS.Data.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

using System.Threading.Tasks;
namespace LMS.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountController(UserManager<User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    // Lấy số dư hiện tại
    public async Task<IActionResult> GetBalance()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var balance = await _context.Transactions
            .Where(t => t.UserId == user.Id)
            .SumAsync(t => t.Amount); // Tổng tất cả giao dịch

        return Json(new { balance = balance });
    }

    // Nạp tiền vào tài khoản
    [HttpPost]
    public async Task<IActionResult> Deposit(decimal amount)
    {
        if (amount <= 0) return BadRequest("Số tiền phải lớn hơn 0.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var transaction = new Transaction
        {
            UserId = user.Id,
            Amount = amount,
            TransactionType = TransactionType.Deposit,
            CreateDate = DateTime.Now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Json(new { success = true, balance = await GetUserBalance(user.Id) });
    }

    // Rút tiền từ tài khoản
    [HttpPost]
    public async Task<IActionResult> Withdraw(decimal amount)
    {
        if (amount <= 0) return BadRequest("Số tiền phải lớn hơn 0.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var balance = await GetUserBalance(user.Id);
        if (balance < amount) return BadRequest("Số dư không đủ.");

        var transaction = new Transaction
        {
            UserId = user.Id,
            Amount = -amount, // Trừ số tiền rút đi
            TransactionType = TransactionType.Withdraw,
            CreateDate = DateTime.Now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Json(new { success = true, balance = await GetUserBalance(user.Id) });
    }

    // Hàm tính số dư hiện tại của user
    private async Task<decimal> GetUserBalance(string userId)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId)
            .SumAsync(t => t.Amount);
    }
    [HttpGet]
    public async Task<IActionResult> TransactionHistory()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var transactions = await _context.Transactions
            .Where(t => t.UserId == user.Id)
            .OrderByDescending(t => t.CreateDate)
            .ToListAsync();

        return View(transactions);
    }

}
