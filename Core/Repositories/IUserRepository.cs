
using LMS.Data.Entities;

namespace LMS.Core.Repositories
{
    public interface IUserRepository
    {
        ICollection<User> GetUsers();

        User GetUser(string id);

        User UpdateUser(User user);
    }
}
