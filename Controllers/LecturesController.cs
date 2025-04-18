using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LMS.Data;
using LMS.Data.Entities;
using LMS.ViewModels;

namespace LMS.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để xem bài giảng
    public class LecturesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturesController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}