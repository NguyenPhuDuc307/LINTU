using LMS.Data;
using LMS.Data.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LMS.ViewModels;
using System.Linq;
using LMS.Services;
public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;
    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await _context.Posts.ToListAsync();
    }
    public async Task<Post> GetPostByIdAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        return post ?? throw new Exception("Bài viết không tồn tại!");
    }
    public async Task<bool> CreatePostAsync(PostCreateRequest request, string userId)
    {
        if(!Guid.TryParse(request.ClassRoomId, out Guid classRoomGuid))
        {
            return false;
        }
        var post = new Post()
        {
            ClassRoomId = classRoomGuid,
            UserId = userId,
            Title = request.Title,
            Message = request.Message,
            CreateDate = DateTime.Now,
            LastModifiedDate = DateTime.Now
        };
        _context.Add(post);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdatePostAsync(Post post)
    {
        if(!PostExists(post.Id))
        {
            return false;
        }
        try
        {
            _context.Update(post);
            await _context.SaveChangesAsync();
            return true;
        }
        catch(DbUpdateConcurrencyException)
        {
            return false;
        }   
    }
    
    public async Task<bool> DeletePostAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if(post == null)
        {
            return false;
        }
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
    public bool PostExists(int id)
    {
        return _context.Posts.Any(e => e.Id == id);
    }
}
