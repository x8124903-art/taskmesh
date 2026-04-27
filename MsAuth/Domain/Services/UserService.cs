using MsAuth.Application.Models;
using MsAuth.Domain.Events;
using MsAuth.Infrastructure.EventBus;

namespace MsAuth.Domain.Services
{
    public sealed class UserService : IUserService
    {
        private readonly MsAuth.Infrastructure.Repositories.IUserRepository _repository;
        private readonly IPasswordHasher _hasher;
        private readonly IEventBus _eventBus;
        private readonly ILogger<UserService> _logger;

        public UserService(
            MsAuth.Infrastructure.Repositories.IUserRepository repository, 
            IPasswordHasher hasher,
            IEventBus eventBus,
            ILogger<UserService> logger)
        {
            _repository = repository;
            _hasher = hasher;
            _eventBus = eventBus;
            _logger = logger;
        }

        public Task<UserModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _repository.GetByIdAsync(id, cancellationToken);
        }

        public Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _repository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<UserModel> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _repository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }
            
            var hash = _hasher.HashPassword(request.Password);
            var user = await _repository.AddAsync(request.Email, request.Name, hash, cancellationToken);

            try
            {
                var evt = new UserRegisteredEvent(
                    EventId: Guid.NewGuid().ToString(),
                    OccurredAt: DateTime.UtcNow,
                    UserId: user.IdUser,
                    Email: user.Email,
                    Name: user.Name
                );
                await _eventBus.PublishAsync(evt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserRegisteredEvent for user {UserId}", user.IdUser);
            }

            return user;
        }

        public async Task<UserModel?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _repository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
                return null;
            if (_hasher.VerifyPassword(password, user.PasswordHash))
                return user;
            return null;
        }
    }
}
