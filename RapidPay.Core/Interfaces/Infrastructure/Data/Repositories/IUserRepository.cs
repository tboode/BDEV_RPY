using RapidPay.Core.Entities;

namespace RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

public interface IUserRepository
{
    public void CreateUser(User user);
    public User? GetUser(Guid userId);
    public bool UserExists(Guid userId);
}