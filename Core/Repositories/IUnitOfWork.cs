
namespace LMS.Core.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }

        IRoleRepository Role { get; }
        // ITransactionRepository Transaction { get; }
        Task CommitAsync();
    }
}
