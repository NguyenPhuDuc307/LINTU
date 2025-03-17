using LMS.Data;
using LMS.Data.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LMS.ViewModels;
using System.Linq;
namespace LMS.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetAllPostsAsync();
        Task<Post> GetPostByIdAsync(int id);
        Task<bool> CreatePostAsync(PostCreateRequest request, string userId);
        Task<bool> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int id);
        bool PostExists(int id);
    }
}