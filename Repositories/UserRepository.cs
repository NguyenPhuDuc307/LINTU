
using LMS.Core.Repositories;
using LMS.Data;
using LMS.Data.Entities;

namespace LMS.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICollection<User> GetUsers()
        {
            return _context.Users.ToList();
        }

        public User GetUser(string id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id)!;
        }

        public User UpdateUser(User user)
        {
             _context.Update(user);
             _context.SaveChanges();

             return user;
        }

    }
}

