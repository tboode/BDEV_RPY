using System.Collections.Concurrent;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

namespace RapidPay.Infrastructure.Data.Repositories;

public class InMemoryUserRepository: IUserRepository
{
    private readonly ConcurrentBag<User> _users = new();
    
    public void CreateUser(User user)
    {
        if (UserExists(user.Id))
            throw new ArgumentException("Attempted to create user that already exists.");
        
        _users.Add(user);
    }

    public User? GetUser(Guid userId)
    {
        return !UserExists(userId) ? null : _users.First(u => u.Id == userId);
    }

    public bool UserExists(Guid userId)
    {
        return _users.Any(u => u.Id == userId);
    }
}