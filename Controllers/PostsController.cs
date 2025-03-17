using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Data.Entities;
using System.Security.Claims;
using LMS.ViewModels;
using LMS.Extensions;
using Microsoft.AspNetCore.Authorization;
using LMS.Services;

namespace LMS.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }
        [Authorize(Roles = "Administrator,Manager")]
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            return View(await _postService.GetAllPostsAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateRequest request)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserId();
                bool isSuccess = await _postService.CreatePostAsync(request, userId);
                if (!isSuccess)
                {
                    return BadRequest("Invalid ClassRoomId.");
                }
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Bài viết đã được tạo thành công!" });
                }

                return RedirectToAction("Details", "ClassRooms", new { id = request.ClassRoomId });
            }

            return View(request);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postService.GetPostByIdAsync(id.Value);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Message,CreateDate,LastModifiedDate")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool isSuccess = await _postService.UpdatePostAsync(post);
                if (!isSuccess)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }
        
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postService.GetPostByIdAsync(id.Value);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isDeleted = await _postService.DeletePostAsync(id);
            if (!isDeleted)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại." });
            }

            return Json(new { success = true, message = "Xóa bài viết thành công!" });
        }

    }
}
