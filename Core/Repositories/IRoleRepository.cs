
using Microsoft.AspNetCore.Identity;

namespace LMS.Core.Repositories
{
    public interface IRoleRepository
    {
        ICollection<IdentityRole> GetRoles();
    }
}
