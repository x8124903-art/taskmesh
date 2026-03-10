using System.Diagnostics.CodeAnalysis;
using MsAuth.Application.Models;

namespace MsAuth.Infrastructure.Repositories
{
    [ExcludeFromCodeCoverage]
    public sealed class UserSqlRepository : IUserRepository
    {
        private readonly MsAuth.Infrastructure.Data.IDapperContext _context;

        public UserSqlRepository(MsAuth.Infrastructure.Data.IDapperContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            return await _context.QueryFirstOrDefaultAsync<UserModel>(
                connection,
                QueriesMySql.GetById,
                new { IdUser = id },
                cancellationToken);
        }

        public async Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            return await _context.QueryFirstOrDefaultAsync<UserModel>(
                connection,
                QueriesMySql.GetByEmail,
                new { Email = email },
                cancellationToken);
        }

        public async Task<UserModel> AddAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            var now = DateTime.UtcNow;
            var passwordHash = request.Password;
            var id = await _context.ExecuteScalarAsync<int>(
                connection,
                QueriesMySql.Insert,
                new { request.Email, request.Name, PasswordHash = passwordHash, CreatedAt = now },
                cancellationToken);
            return new UserModel(id, request.Email, request.Name, passwordHash, false, now);
        }
    }
}