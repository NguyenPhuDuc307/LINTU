using LMS.Core.Repositories;
using LMS.Data;
namespace LMS.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserRepository User { get; }
        public IRoleRepository Role { get; }

        public UnitOfWork(ApplicationDbContext context,IUserRepository user, IRoleRepository role)
        {
            _context = context;
            User = user;
            Role = role;
        }
        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
