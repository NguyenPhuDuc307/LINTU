using LMS.Data;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Areas.Identity.Pages.Account.Manage
{
    public class TransactionHistoryModel(ApplicationDbContext context, UserManager<User> userManager) : PageModel
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<User> _userManager = userManager;

        public required List<Transaction> Transactions { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Transactions = new List<Transaction>();
                return;
            }

            Transactions = await _context.Transactions
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.CreateDate)
                .ToListAsync();
        }
    }
}
