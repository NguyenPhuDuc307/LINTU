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
using Microsoft.AspNetCore.Http;
using LMS.Models.ViewModels;

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
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;
            var totalItems = await _context.ClassRooms.CountAsync();
            if (totalItems == 0)
            {
                ViewBag.NoClassMessage = "Không có lớp học nào.";
                return View(new List<ClassRoom>());
            }
            var classRooms = await _context.ClassRooms
                .Include(c => c.Topic)
                .OrderBy(c => c.Status)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get the number of students in each class through a separate query
            var studentCounts = await _context.ClassDetails
                .GroupBy(cd => cd.ClassRoomId)
                .Select(g => new { ClassRoomId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ClassRoomId, x => x.Count);

            // Assign student count to each class room in the list
            foreach (var classRoom in classRooms)
            {
                classRoom.Students = studentCounts.ContainsKey(classRoom.Id) ? studentCounts[classRoom.Id] : 0;
            }
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            return View(classRooms);
        }
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public async Task<IActionResult> ApproveClassRoom(Guid id)
        {
            var classRoom = await _context.ClassRooms.FindAsync(id);
            if (classRoom == null)
            {
                return NotFound();
            }

            if (classRoom.Status == ClassRoomStatus.Pending)
            {
                classRoom.Status = ClassRoomStatus.Approved;
                _context.Update(classRoom);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

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
        // GET: ClassRooms/Details/5
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

            var assignments = await _context.Assignments
                .Where(a => a.ClassRoomId == classRoom.Id)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            var participants = await _context.ClassDetails
                .Where(cd => cd.ClassRoomId == classRoom.Id)
                .Select(cd => cd.User)
                .ToListAsync();

            var classRoomViewModel = new ClassRoomViewModel
            {
                ClassRoom = classRoom,
                Posts = posts,
                MembersCount = membersCount,
                Assignments = assignments,
                Participants = participants!
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
            if (classRoom == null)
            {
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

            var classDetail = await _context.ClassDetails
                .FirstOrDefaultAsync(cd => cd.ClassRoomId == classRoom.Id && cd.UserId == user.Id);

            if (classDetail != null)
            {
                classDetail.IsPaid = true;
                _context.Update(classDetail);
                await _context.SaveChangesAsync();
            }
            // Thêm giao dịch vào Transaction
            var transaction = new Transaction
            {
                UserId = user.Id,
                Amount = (decimal)-classRoom.Price,  // Trừ tiền khi thanh toán
                TransactionType = TransactionType.Withdraw, // Hoặc TransactionType.Payment nếu bạn có loại này
                CreateDate = DateTime.Now
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
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
            var userId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                classRoom.UserId = userId;
                classRoom.Code = GenerateRandomCode(6);
                classRoom.Students = 0;
                classRoom.CreateDate = DateTime.Now;

                _context.Add(classRoom);
                await _context.SaveChangesAsync();

                var classDetail = new ClassDetail
                {
                    ClassRoomId = classRoom.Id,
                    UserId = userId,
                    CreateDate = DateTime.Now,
                    IsPaid = true
                };

                _context.ClassDetails.Add(classDetail);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = classRoom.Id });
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
            ViewData["TopicList"] = new SelectList(_context.Topics, "Id", "Name", classRoom.TopicId);
            return View(classRoom);
        }

        // POST: ClassRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ClassRoom classRoom, IFormFile? ImageFile)
        {
            if (id != classRoom.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingClassRoom = await _context.ClassRooms.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingClassRoom == null)
                    {
                        return NotFound();
                    }
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        classRoom.ImageUrl = "/uploads/" + fileName;
                    }
                    else
                    {
                        classRoom.ImageUrl = existingClassRoom.ImageUrl;
                    }
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
            ViewData["TopicList"] = new SelectList(_context.Topics, "Id", "Name", classRoom.TopicId);
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
        public async Task<IActionResult> DeleteConfirmed(Guid id)
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
        [Authorize]
        public async Task<IActionResult> Teach(int page = 1)
        {

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            int pageSize = 6;
            var totalItems = await _context.ClassRooms
                .Where(cd => cd.UserId == userId)
                .CountAsync();

            if (totalItems == 0)
            {
                ViewBag.NoClassMessage = "Bạn chưa tạo lớp học nào.";
                return View(new List<ClassRoom>());
            }
            var classes = await _context.ClassRooms
                .Where(c => c.UserId == userId)
                .Include(c => c.Topic)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            return View(classes);
        }
        [Authorize]
        public async Task<IActionResult> Registered(int page = 1)
        {

            var userId = _userManager.GetUserId(User);
            int pageSize = 6;
            // Lấy tổng số lớp học người dùng đã đăng ký
            var totalItems = await _context.ClassDetails
                .Where(cd => cd.UserId == userId)
                .CountAsync();


            if (totalItems == 0)
            {
                ViewBag.NoClassMessage = "Bạn chưa đăng ký lớp học nào.";
                return View(new List<RegisteredClassViewModel>());
            }
            // Lấy các lớp mà người dùng đã tham gia hoặc đã mua
            var registeredClasses = await _context.ClassDetails
                .Where(cd => cd.UserId == userId && cd.ClassRoom != null)
                .Include(cd => cd.ClassRoom)
                .ThenInclude(c => c!.Topic)
                .Select(cd => new RegisteredClassViewModel
                {
                    ClassRoom = cd.ClassRoom!,
                    IsPaid = cd.IsPaid // Kiểm tra trạng thái đã thanh toán
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Truyền dữ liệu phân trang đến View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            return View(registeredClasses);
        }
        [Authorize]
        public async Task<IActionResult> GetLatestAssignments()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not found." });
            }

            var latestAssignments = await _context.Assignments
                .Where(a => _context.ClassDetails.Any(cd => cd.UserId == userId && cd.ClassRoomId == a.ClassRoomId))
                .Include(a => a.ClassRoom) // Load ClassRoom
                .OrderByDescending(a => a.DueDate)
                .Take(3)
                .Select(a => new
                {
                    ClassName = a.ClassRoom != null ? a.ClassRoom.Name : "Unknown Class",
                    ClassImage = a.ClassRoom != null && !string.IsNullOrEmpty(a.ClassRoom.ImageUrl)
                                 ? a.ClassRoom.ImageUrl
                                 : "/files/assets/images/default-class.png",
                    AssignmentTitle = !string.IsNullOrEmpty(a.Title) ? a.Title : "No Title",
                    DueDate = a.DueDate.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            Console.WriteLine($"Total Assignments fetched: {latestAssignments.Count}");
            latestAssignments.ForEach(a => Console.WriteLine($"Class: {a.ClassName}, Title: {a.AssignmentTitle}, Due: {a.DueDate}"));

            if (!latestAssignments.Any())
            {
                return Json(new { success = false, message = "No assignments found." });
            }

            return Json(new { success = true, assignments = latestAssignments });
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

                classroom.Students = await _context.ClassDetails.CountAsync(cd => cd.ClassRoomId == classroom.Id);
                _context.Update(classroom);
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
