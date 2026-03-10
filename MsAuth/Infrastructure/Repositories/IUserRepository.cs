using MsAuth.Application.Models;

namespace MsAuth.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<UserModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserModel> AddAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    }
}