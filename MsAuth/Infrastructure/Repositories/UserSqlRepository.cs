using System.Diagnostics.CodeAnalysis;
using MsAuth.Application.Models;
using MsAuth.Infrastructure.Persistence;

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
            var entity = await _context.QueryFirstOrDefaultAsync<UserEntity>(
                connection,
                QueriesMySql.GetById,
                new { IdUser = id },
                cancellationToken);
            return entity is null ? null : ToModel(entity);
        }

        public async Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            var entity = await _context.QueryFirstOrDefaultAsync<UserEntity>(
                connection,
                QueriesMySql.GetByEmail,
                new { Email = email },
                cancellationToken);
            return entity is null ? null : ToModel(entity);
        }

        public async Task<UserModel> AddAsync(string email, string name, string passwordHash, CancellationToken cancellationToken = default)
        {
            using var connection = _context.CreateConnection();
            var now = DateTime.UtcNow;
            var id = await _context.ExecuteScalarAsync<int>(
                connection,
                QueriesMySql.Insert,
                new { Email = email, Name = name, PasswordHash = passwordHash, CreatedAt = now },
                cancellationToken);
            return new UserModel(id, email, name, passwordHash, false, now);
        }

        private static UserModel ToModel(UserEntity e) =>
            new(e.IdUser, e.Email, e.Name, e.PasswordHash, e.IsDeleted, e.CreatedAt);
    }
}