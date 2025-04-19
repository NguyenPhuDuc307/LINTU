using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using LMS.Data;
using LMS.Data.Entities;
using LMS.ViewModels;

namespace LMS.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để xem bài giảng
    public class LecturesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LecturesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Lectures/Watch/5
        public async Task<IActionResult> Watch(int id)
        {
            // Lấy thông tin về lecture hiện tại
            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            // Lấy thông tin về classroom
            var classroom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Id == lecture.ClassRoomId);

            if (classroom == null)
            {
                return NotFound();
            }

            // Lấy tất cả các lesson trong khóa học, bao gồm các lecture
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == lecture.ClassRoomId)
                .OrderBy(l => l.Order)
                .Include(l => l.Lectures.OrderBy(lec => lec.Order))
                .ToListAsync();

            // Lấy danh sách các lecture đã hoàn thành
            var userId = User.Identity?.Name;
            var completedLectures = await _context.CompletedLectures
                .Where(cl => cl.UserId == userId && cl.ClassRoomId == lecture.ClassRoomId)
                .Select(cl => cl.LectureId)
                .ToListAsync();

            // Tính tiến độ hoàn thành khóa học
            int totalLectures = lessons.Sum(l => l.Lectures.Count);
            int progress = totalLectures > 0
                ? (completedLectures.Count * 100) / totalLectures
                : 0;

            // Xác định lecture trước và lecture sau
            Lecture? previousLecture = null;
            Lecture? nextLecture = null;

            // Logic để xác định lecture trước và sau
            bool foundCurrent = false;
            foreach (var lesson in lessons.OrderBy(l => l.Order))
            {
                foreach (var lec in lesson.Lectures.OrderBy(l => l.Order))
                {
                    if (foundCurrent)
                    {
                        nextLecture = lec;
                        break;
                    }

                    if (lec.Id == id)
                    {
                        foundCurrent = true;
                    }
                    else
                    {
                        previousLecture = lec;
                    }
                }

                if (nextLecture != null)
                {
                    break;
                }
            }

            // Tạo viewmodel
            var viewModel = new LectureViewModel
            {
                Lecture = lecture,
                Lesson = lecture.Lesson!,
                ClassRoom = classroom,
                Lessons = lessons,
                CompletedLectures = completedLectures,
                PreviousLecture = previousLecture,
                NextLecture = nextLecture,
                CourseProgress = progress,
                // Example resources
                Resources = new List<ResourceViewModel>
                {
                    new ResourceViewModel
                    {
                        Title = "Lecture Slides",
                        Url = "/files/slides.pdf"
                    },
                    new ResourceViewModel
                    {
                        Title = "Exercise Files",
                        Url = "/files/exercises.zip"
                    }
                }
            };

            // Ghi lại việc xem bài giảng này (có thể làm thông qua AJAX)
            // await LogLectureView(id, userId);

            return View(viewModel);
        }

        // POST: Lectures/MarkAsCompleted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCompleted(int lectureId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture == null)
            {
                return NotFound();
            }

            // Kiểm tra xem đã đánh dấu hoàn thành chưa
            var completed = await _context.CompletedLectures
                .AnyAsync(cl => cl.LectureId == lectureId && cl.UserId == userId);

            if (!completed)
            {
                // Thêm mới nếu chưa hoàn thành
                _context.CompletedLectures.Add(new CompletedLecture
                {
                    LectureId = lectureId,
                    UserId = userId,
                    ClassRoomId = lecture.ClassRoomId,
                    CompletedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: Lectures/MarkAsNotCompleted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsNotCompleted(int lectureId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var completedLecture = await _context.CompletedLectures
                .FirstOrDefaultAsync(cl => cl.LectureId == lectureId && cl.UserId == userId);

            if (completedLecture != null)
            {
                _context.CompletedLectures.Remove(completedLecture);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: Lectures/SaveNotes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotes(int lectureId, string notes)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lectureNote = await _context.LectureNotes
                .FirstOrDefaultAsync(ln => ln.LectureId == lectureId && ln.UserId == userId);

            if (lectureNote == null)
            {
                // Thêm mới nếu chưa có ghi chú
                var lecture = await _context.Lectures.FindAsync(lectureId);
                if (lecture == null)
                {
                    return NotFound();
                }

                _context.LectureNotes.Add(new LectureNote
                {
                    LectureId = lectureId,
                    UserId = userId,
                    ClassRoomId = lecture.ClassRoomId,
                    Notes = notes,
                    UpdatedDate = DateTime.Now
                });
            }
            else
            {
                // Cập nhật ghi chú nếu đã có
                lectureNote.Notes = notes;
                lectureNote.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: Lectures/Create
        public async Task<IActionResult> Create(int? lessonId, Guid? classRoomId)
        {
            if (lessonId == null && classRoomId == null)
            {
                return NotFound();
            }

            var viewModel = new LectureCreateViewModel();

            if (classRoomId.HasValue)
            {
                var classRoom = await _context.ClassRooms.FindAsync(classRoomId);
                if (classRoom == null)
                {
                    return NotFound();
                }

                viewModel.ClassRoomId = classRoomId.Value;

                // Get lessons for dropdown
                var lessons = await _context.Lessons
                    .Where(l => l.ClassRoomId == classRoomId)
                    .OrderBy(l => l.Order)
                    .ToListAsync();

                viewModel.LessonList = new SelectList(lessons, "Id", "Title");

                if (lessonId.HasValue)
                {
                    viewModel.LessonId = lessonId.Value;
                }
                else if (lessons.Any())
                {
                    viewModel.LessonId = lessons.First().Id;
                }
            }

            return View(viewModel);
        }

        // POST: Lectures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LectureCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var lesson = await _context.Lessons.FindAsync(viewModel.LessonId);
                if (lesson == null)
                {
                    return NotFound();
                }

                var lecture = new Lecture
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Order = viewModel.Order,
                    LessonId = viewModel.LessonId,
                    ClassRoomId = lesson.ClassRoomId,
                    ContentType = viewModel.ContentType,
                    DurationMinutes = viewModel.DurationMinutes,
                    CreateDate = DateTime.Now
                };

                // Handle content based on type
                if (viewModel.ContentType == LectureContentType.VideoUrl)
                {
                    lecture.VideoUrl = viewModel.VideoUrl;
                }
                else if (viewModel.ContentType == LectureContentType.UploadedFile && viewModel.UploadedFile != null)
                {
                    // Save the uploaded file
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.UploadedFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.UploadedFile.CopyToAsync(fileStream);
                    }

                    lecture.FileUrl = "/uploads/lectures/" + uniqueFileName;
                }
                else if (viewModel.ContentType == LectureContentType.TextContent)
                {
                    lecture.TextContent = viewModel.TextContent;
                }

                _context.Add(lecture);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ClassRooms", new { id = lesson.ClassRoomId });
            }

            // If we got this far, something failed, redisplay form
            if (viewModel.ClassRoomId != Guid.Empty)
            {
                var lessons = await _context.Lessons
                    .Where(l => l.ClassRoomId == viewModel.ClassRoomId)
                    .OrderBy(l => l.Order)
                    .ToListAsync();

                viewModel.LessonList = new SelectList(lessons, "Id", "Title", viewModel.LessonId);
            }

            return View(viewModel);
        }

        // GET: Lectures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            var viewModel = new LectureEditViewModel
            {
                Id = lecture.Id,
                Title = lecture.Title,
                Description = lecture.Description,
                Order = lecture.Order,
                LessonId = lecture.LessonId,
                ClassRoomId = lecture.ClassRoomId,
                ContentType = lecture.ContentType,
                VideoUrl = lecture.VideoUrl,
                FileUrl = lecture.FileUrl,
                TextContent = lecture.TextContent,
                DurationMinutes = lecture.DurationMinutes
            };

            // Get lessons for dropdown
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == lecture.ClassRoomId)
                .OrderBy(l => l.Order)
                .ToListAsync();

            viewModel.LessonList = new SelectList(lessons, "Id", "Title", lecture.LessonId);

            return View(viewModel);
        }

        // POST: Lectures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LectureEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var lecture = await _context.Lectures.FindAsync(id);
                    if (lecture == null)
                    {
                        return NotFound();
                    }

                    lecture.Title = viewModel.Title;
                    lecture.Description = viewModel.Description;
                    lecture.Order = viewModel.Order;
                    lecture.LessonId = viewModel.LessonId;
                    lecture.ContentType = viewModel.ContentType;
                    lecture.DurationMinutes = viewModel.DurationMinutes;
                    lecture.LastModifiedDate = DateTime.Now;

                    // Handle content based on type
                    if (viewModel.ContentType == LectureContentType.VideoUrl)
                    {
                        lecture.VideoUrl = viewModel.VideoUrl;
                        lecture.FileUrl = null;
                        lecture.TextContent = null;
                    }
                    else if (viewModel.ContentType == LectureContentType.UploadedFile && viewModel.UploadedFile != null)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(lecture.FileUrl))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, lecture.FileUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save the new uploaded file
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.UploadedFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.UploadedFile.CopyToAsync(fileStream);
                        }

                        lecture.FileUrl = "/uploads/lectures/" + uniqueFileName;
                        lecture.VideoUrl = null;
                        lecture.TextContent = null;
                    }
                    else if (viewModel.ContentType == LectureContentType.TextContent)
                    {
                        lecture.TextContent = viewModel.TextContent;
                        lecture.VideoUrl = null;
                        lecture.FileUrl = null;
                    }

                    _context.Update(lecture);
                    await _context.SaveChangesAsync();

                    // Get the lesson to redirect back to the lessons page
                    var lesson = await _context.Lessons.FindAsync(lecture.LessonId);
                    if (lesson == null)
                    {
                        return NotFound();
                    }

                    return RedirectToAction("Details", "ClassRooms", new { id = lesson.ClassRoomId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LectureExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == viewModel.ClassRoomId)
                .OrderBy(l => l.Order)
                .ToListAsync();

            viewModel.LessonList = new SelectList(lessons, "Id", "Title", viewModel.LessonId);

            return View(viewModel);
        }

        // GET: Lectures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // POST: Lectures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }

            // Delete file if exists
            if (!string.IsNullOrEmpty(lecture.FileUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, lecture.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Get the lesson to redirect back to the lessons page
            var lesson = await _context.Lessons.FindAsync(lecture.LessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            var classRoomId = lesson.ClassRoomId;

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ClassRooms", new { id = classRoomId });
        }

        private bool LectureExists(int id)
        {
            return _context.Lectures.Any(e => e.Id == id);
        }
    }
}