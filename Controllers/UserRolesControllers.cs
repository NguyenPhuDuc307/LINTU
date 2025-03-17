
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LMS.Core.Repositories;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using LMS.Core.ViewModels;
using LMS.Data;

namespace LMS.Controllers
{ 
    public class UserRolesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        public UserRolesController(IUnitOfWork unitOfWork, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _context = context;
        }
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Index()
        {
            var users = _unitOfWork.User.GetUsers();
            return View(users);
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = _unitOfWork.User.GetUser(id);
            var roles = _unitOfWork.Role.GetRoles();

            var userRoles = await _signInManager.UserManager.GetRolesAsync(user);

            var roleItems = roles.Select(role =>
                new SelectListItem(
                    role.Name,
                    role.Id,
                    userRoles.Any(ur => ur.Contains(role.Name!)))).ToList();

            var vm = new EditUserViewModel
            {
                User = user,
                Roles = roleItems
            };

            return View(vm);
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = _unitOfWork.User.GetUser(id);
            
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            var userRolesInDb = await _signInManager.UserManager.GetRolesAsync(user);
            if (userRolesInDb != null && userRolesInDb.Any())
            {
                await _signInManager.UserManager.RemoveFromRolesAsync(user, userRolesInDb);
            }
            // Xóa các bài viết liên quan trước
            var posts = _context.Posts.Where(p => p.UserId == user.Id);
            _context.Posts.RemoveRange(posts);
            // Xóa các giao dịch liên quan trước
            var transactions = _context.Transactions.Where(t => t.UserId == user.Id);
            _context.Transactions.RemoveRange(transactions);

            await _context.SaveChangesAsync();
            _unitOfWork.User.DeleteUser(user);

            await _unitOfWork.CommitAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(EditUserViewModel data)
        {
            if (data?.User == null || data.Roles == null)
            {
                return BadRequest("Invalid data.");
            }
            var user = _unitOfWork.User.GetUser(data.User!.Id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userRolesInDb = await _signInManager.UserManager.GetRolesAsync(user);
            if (userRolesInDb == null)
            {
                return StatusCode(500, "Failed to retrieve user roles.");
            }
            var rolesToAdd = new List<string>();
            var rolesToDelete = new List<string>();

            foreach (var role in data.Roles!)
            {
                var assignedInDb = userRolesInDb.FirstOrDefault(ur => ur == role.Text);
                if (role.Selected)
                {
                    if (assignedInDb == null)
                    {
                        rolesToAdd.Add(role.Text);
                    }
                }
                else
                {
                    if (assignedInDb != null)
                    {
                        rolesToDelete.Add(role.Text);
                    }
                }
            }

            if (rolesToAdd.Any())
            {
                await _signInManager.UserManager.AddToRolesAsync(user, rolesToAdd);
            }

            if (rolesToDelete.Any())
            {
                await _signInManager.UserManager.RemoveFromRolesAsync(user, rolesToDelete);
            }

            user.FullName = data.User.FullName;
            user.Email = data.User.Email;

            _unitOfWork.User.UpdateUser(user);

            return RedirectToAction("Edit", new { id = user.Id });
        }
    }
}
