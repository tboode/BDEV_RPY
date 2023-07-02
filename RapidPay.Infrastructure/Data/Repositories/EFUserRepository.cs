using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

namespace RapidPay.Infrastructure.Data.Repositories;

public class EFUserRepository: IUserRepository
{
    private readonly RapidPayDbContext _dbContext;

    public EFUserRepository(RapidPayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void CreateUser(User user)
    {
        _dbContext.Add(user);
        _dbContext.SaveChanges();
    }

    public User? GetUser(Guid userId)
    {
        return _dbContext.Users.FirstOrDefault(x => x.Id.Equals(userId));
    }

    public bool UserExists(Guid userId)
    {
        return _dbContext.Users.Any(x => x.Id.Equals(userId));
    }
}