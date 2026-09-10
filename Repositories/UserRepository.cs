using APICargadores.Config;
using APICargadores.Models;
using MongoDB.Driver;

namespace APICargadores.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext context) => _users = context.Users;

    public async Task<User?> GetByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User> CreateAsync(User user)
    {
        user.Email = user.Email.ToLowerInvariant();
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> ExistsAsync(string email) =>
        await _users.Find(u => u.Email == email.ToLowerInvariant()).AnyAsync();
}
