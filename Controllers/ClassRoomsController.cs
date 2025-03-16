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
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace LMS.Controllers
{
    public class ClassRoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ClassRoomsController> _logger;

        public ClassRoomsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<ClassRoomsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }
        [Authorize(Roles = "Administrator,Manager")]
        // GET: ClassRooms
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ClassRooms.Include(c => c.Topic);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ClassRooms/Details/5

        public async Task<IActionResult> Introduction(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("ID is null or empty.");
                return NotFound();
            }

            if (!Guid.TryParse(id, out Guid classRoomGuid))
            {
                _logger.LogWarning($"Invalid GUID format: {id}");
                return NotFound();
            }
            var classRoom = await _context.ClassRooms
                .Include(c => c.Topic) 
                .FirstOrDefaultAsync(m => m.Id == classRoomGuid);
            if (classRoom == null)
            {
                _logger.LogWarning($"ClassRoom not found for ID: {classRoomGuid}");
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            ViewBag.IsJoined = user != null && await _context.ClassDetails
                .AnyAsync(cd => cd.ClassRoomId == classRoom.Id && cd.UserId == user.Id);

            ViewBag.MembersCount = await _context.ClassDetails
                .CountAsync(cd => cd.ClassRoomId == classRoom.Id);
            return View(classRoom);
        }

        [Route("classroom/{id}")]
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid classRoomGuid))
            {
                return NotFound();
            }
            var classRoom = await _context.ClassRooms
                .Include(c => c.Topic)
                .FirstOrDefaultAsync(m => m.Id == classRoomGuid);
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
        public async Task<IActionResult> JoinFreeClass(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid classRoomGuid))
            {
                return Json(new { success = false, message = "Mã lớp không hợp lệ!" });
            }
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == classRoomGuid);
            if (classRoom == null){
                return Json(new { success = false, message = "Không tìm thấy lớp học!" });
            } 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            { 
                return Json(new { success = false, message = "Bạn cần đăng nhập để tham gia lớp!" }); 
            }

            bool alreadyJoined = await _context.ClassDetails
                .AnyAsync(cd => cd.ClassRoomId == classRoom.Id && cd.UserId == user.Id);

            if (!alreadyJoined)
            {
                await AddUserToClassroom(classRoom, user.Id);
            }

            return RedirectToAction("Details", new { id = classRoom.Id });
        }

        public IActionResult PaymentConfirmation(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid classRoomGuid))
            {
                return NotFound();
            }
            var classRoom = _context.ClassRooms.FirstOrDefault(c => c.Id == classRoomGuid);
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
        public async Task<IActionResult> ConfirmPayment(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid classRoomGuid))
            {
                return NotFound();
            }
            var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == classRoomGuid);
            if (classRoom == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();
            
            if (classRoom.Price == 0)
            {
                await AddUserToClassroom(classRoom, user.Id);
                return RedirectToAction("Details", new { id = classRoom.Id });
            }

            decimal userBalance = await GetUserBalance(user.Id);

            if (userBalance < (decimal)classRoom.Price)
            {
                TempData["ErrorMessage"] = "Bạn không đủ tiền để mua khóa học. Vui lòng nạp thêm!";
                return RedirectToAction("Index", "Pays", new { id = classRoom.Id });
            }
            await ProcessPayment(user.Id, classRoom.Price);
            await AddUserToClassroom(classRoom, user.Id);

            return RedirectToAction("Details", new { id = classRoom.Id });
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
        public async Task<IActionResult> Create([Bind("Name,TopicId,Introduction,Description,ImageUrl,Price")] ClassRoom classRoom)
        {
            if (ModelState.IsValid)
            {
                classRoom.Code = GenerateRandomCode(6);
                classRoom.Students = 0;
                classRoom.CreateDate = DateTime.Now;
                _context.Add(classRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TopicId"] = new SelectList(_context.Topics, "Id", "Name", classRoom.TopicId);
            return View(classRoom);
        }
        [Authorize(Roles = "Administrator,Manager")]
        // GET: ClassRooms/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
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
        public async Task<IActionResult> Edit(Guid id, ClassRoom classRoom)
        {
            if (id != classRoom.Id)
            {
                return BadRequest();
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
                    if (!ClassRoomExists(classRoom.Id.ToString()))
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
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(Guid? id)
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
        private bool ClassRoomExists(string id)
        {
            return Guid.TryParse(id, out Guid classRoomGuid) && _context.ClassRooms.Any(e => e.Id == classRoomGuid);
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
        }
        [HttpPost]
        public async Task<IActionResult> ChangeCode(Guid id)
        {
            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null)
            {
                return NotFound();
            }

            string newCode;
            do
            {
                newCode = GenerateRandomCode(6); 
            } while (await _context.ClassRooms.AnyAsync(c => c.Code == newCode)); 

            classRoom.Code = newCode;
            _context.Update(classRoom);
            await _context.SaveChangesAsync();

            return Json(new { success = true, newCode = classRoom.Code });
        }

        private static string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
