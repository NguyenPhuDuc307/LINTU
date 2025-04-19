using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LMS.Data;
using LMS.Data.Entities;
using LMS.ViewModels;

namespace LMS.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Lessons
        public async Task<IActionResult> Index(Guid? classRoomId)
        {
            if (classRoomId == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Id == classRoomId);

            if (classRoom == null)
            {
                return NotFound();
            }

            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == classRoomId)
                .OrderBy(l => l.Order)
                .Include(l => l.Lectures.OrderBy(lec => lec.Order))
                .ToListAsync();

            var viewModel = new LessonsIndexViewModel
            {
                ClassRoom = classRoom,
                Lessons = lessons
            };

            return View(viewModel);
        }

        // GET: Lessons/Create
        public IActionResult Create(Guid classRoomId)
        {
            var viewModel = new LessonCreateViewModel
            {
                ClassRoomId = classRoomId
            };
            return View(viewModel);
        }

        // POST: Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var lesson = new Lesson
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Order = viewModel.Order,
                    ClassRoomId = viewModel.ClassRoomId,
                    CreateDate = DateTime.Now
                };

                _context.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ClassRooms", new { id = viewModel.ClassRoomId });
            }
            return View(viewModel);
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var viewModel = new LessonCreateViewModel
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Order = lesson.Order,
                ClassRoomId = lesson.ClassRoomId
            };

            return View(viewModel);
        }

        // POST: Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LessonCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var lesson = await _context.Lessons.FindAsync(id);
                    if (lesson == null)
                    {
                        return NotFound();
                    }

                    lesson.Title = viewModel.Title;
                    lesson.Description = viewModel.Description;
                    lesson.Order = viewModel.Order;
                    lesson.LastModifiedDate = DateTime.Now;

                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "ClassRooms", new { id = viewModel.ClassRoomId });
            }
            return View(viewModel);
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.ClassRoom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var classRoomId = lesson.ClassRoomId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ClassRooms", new { id = classRoomId });
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }

        // POST: Lessons/CreateAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(LessonCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm thứ tự cao nhất hiện tại để thêm bài học mới vào cuối
                    int maxOrder = 0;
                    var lessons = await _context.Lessons
                        .Where(l => l.ClassRoomId == viewModel.ClassRoomId)
                        .ToListAsync();

                    if (lessons.Any())
                    {
                        maxOrder = lessons.Max(l => l.Order);
                    }

                    var lesson = new Lesson
                    {
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        Order = maxOrder + 1, // Thêm vào cuối
                        ClassRoomId = viewModel.ClassRoomId,
                        CreateDate = DateTime.Now
                    };

                    _context.Add(lesson);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Bài học đã được tạo thành công!", lessonId = lesson.Id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi tạo bài học: " + ex.Message });
                }
            }

            // Nếu ModelState không hợp lệ, trả về lỗi
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
        }
    }
}
