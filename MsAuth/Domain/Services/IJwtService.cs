using MsAuth.Application.Models;

namespace MsAuth.Domain.Services;

public interface IJwtService
{
    string GenerateToken(UserModel user);
}
