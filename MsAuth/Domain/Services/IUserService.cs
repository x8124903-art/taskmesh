using MsAuth.Application.Models;

namespace MsAuth.Domain.Services
{
    public interface IUserService
    {
        Task<UserModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserModel> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<UserModel?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
    }
}