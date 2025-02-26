using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Data.Entities;
using LMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using LMS.Data.Entities.Enums;

namespace LMS.Controllers
{
    public class ClassRoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ClassRoomsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ClassRooms
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClassRooms.Include(c => c.Topic);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClassRooms/Details/5

        public async Task<IActionResult> Introduction(string? code)
        {
            if (code == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(m => m.Code == code);
            if (classRoom == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                bool isAlreadyJoined = await _context.ClassDetails
                    .AnyAsync(cd => cd.ClassRoomId == classRoom.Id && cd.UserId == user.Id);
                ViewBag.IsJoined = isAlreadyJoined;
            }
            else
            {
                ViewBag.IsJoined = false; // Nếu chưa đăng nhập, mặc định chưa tham gia
            }
            // Đếm số lượng thành viên chính xác từ ClassDetails
            ViewBag.MembersCount = await _context.ClassDetails
                .Where(cd => cd.ClassRoomId == classRoom.Id)
                .CountAsync();
            return View(classRoom);
        }

        [Route("classroom/{code}")]
        public async Task<IActionResult> Details(string? code)
        {
            if (code == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRooms
                .Include(c => c.Topic)
                .FirstOrDefaultAsync(m => m.Code == code);
            if (classRoom == null)
            {
                return NotFound();
            }

            var posts = await _context.Posts.Where(p => p.ClassRoomId == classRoom.Id).OrderByDescending(p => p.CreateDate).ToListAsync();
            
            int membersCount = await _context.ClassDetails
                .Where(cd => cd.ClassRoomId == classRoom.Id)
                .CountAsync();
            var classRoomViewModel = new ClassRoomViewModel
            {
                ClassRoom = classRoom,
                Posts = posts,
                MembersCount = membersCount
            };
            return View(classRoomViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> JoinFreeClass(string code)
        {
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Code == code);
            if (classRoom == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Kiểm tra nếu user đã tham gia khóa học chưa
            bool alreadyJoined = await _context.ClassDetails
                .AnyAsync(cd => cd.ClassRoomId == classRoom.Id && cd.UserId == user.Id);

            if (!alreadyJoined)
            {
                // Nếu chưa tham gia, thêm vào ClassDetails
                await AddUserToClassroom(classRoom, user.Id);
            }

            return RedirectToAction("Details", new { code = classRoom.Code });
        }

        public IActionResult PaymentConfirmation(string code)
        {
            var classRoom = _context.ClassRooms.FirstOrDefault(c => c.Code == code);
            if (classRoom == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ConfirmMessage = $"Bạn có muốn thanh toán {classRoom.Price:0,0} VND để tham gia lớp {classRoom.Name} không?";
            ViewBag.ClassCode = classRoom.Code;
            return View();
        }

        // Xử lý sau khi thanh toán thành công
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(string code)
        {
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Code == code);
            if (classRoom == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            // Nếu khóa học miễn phí, tự động thêm user vào lớp học mà không cần thanh toán
            if (classRoom.Price == 0)
            {
                await AddUserToClassroom(classRoom, user.Id);
                return RedirectToAction("Details", new { code = classRoom.Code });
            }

            decimal userBalance = await GetUserBalance(user.Id);

            if (userBalance < (decimal)classRoom.Price)
            {
                // Nếu không đủ tiền, chuyển sang trang nạp tiền
                return RedirectToAction("Index", "Pays", new { code = classRoom.Code });
            }
            await ProcessPayment(user.Id, classRoom.Price);
            await AddUserToClassroom(classRoom, user.Id);

            return RedirectToAction("Details", new { code = classRoom.Code });
        }
        // GET: ClassRooms/Create
        public IActionResult Create()
        {
            ViewData["TopicId"] = new SelectList(_context.Topics, "Id", "Name");
            return View();
        }
        // POST: ClassRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,TopicId,Description,ImageUrl,Code,Price,Students")] ClassRoom classRoom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(classRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TopicId"] = new SelectList(_context.Topics, "Id", "Name", classRoom.TopicId);
            return View(classRoom);
        }

        // GET: ClassRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null)
            {
                return NotFound();
            }
            ViewData["TopicId"] = new SelectList(_context.Topics, "Id", "Id", classRoom.TopicId);
            return View(classRoom);
        }

        // POST: ClassRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,TopicId,Description,ImageUrl,Code,Price,Students")] ClassRoom classRoom)
        {
            if (id != classRoom.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(classRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassRoomExists(classRoom.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TopicId"] = new SelectList(_context.Topics, "Id", "Id", classRoom.TopicId);
            return View(classRoom);
        }

        // GET: ClassRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRooms
                .Include(c => c.Topic)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classRoom == null)
            {
                return NotFound();
            }

            return View(classRoom);
        }

        // POST: ClassRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom != null)
            {
                _context.ClassRooms.Remove(classRoom);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // **Hàm trừ tiền và cập nhật số dư**
        private async Task ProcessPayment(string userId, double amount)
        {
            var transaction = new Transaction
            {
                UserId = userId,
                Amount = -((decimal)amount),
                TransactionType = TransactionType.Withdraw
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        // **Hàm lấy số dư của user**
        private async Task<decimal> GetUserBalance(string userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .SumAsync(t => t.Amount);
        }
        private bool ClassRoomExists(int id)
        {
            return _context.ClassRooms.Any(e => e.Id == id);
        }
        // Hàm chung để thêm user vào lớp học
        private async Task AddUserToClassroom(ClassRoom classroom, string userId)
        {
            bool alreadyJoined = await _context.ClassDetails
            .AnyAsync(cd => cd.ClassRoomId == classroom.Id && cd.UserId == userId);

            if (!alreadyJoined)
            {
                var classDetail = new ClassDetail
                {
                    ClassRoomId = classroom.Id,
                    UserId = userId,
                    CreateDate = DateTime.Now
                };

                _context.ClassDetails.Add(classDetail);
                await _context.SaveChangesAsync();
            }
            // var classDetail = new ClassDetail
            // {
            //     ClassRoomId = classroom.Id,
            //     UserId = userId,
            //     CreateDate = DateTime.Now
            // };

            // _context.ClassDetails.Add(classDetail);
            // await _context.SaveChangesAsync();
        }
    }
}
