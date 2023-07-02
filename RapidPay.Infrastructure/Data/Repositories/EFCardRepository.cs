using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

namespace RapidPay.Infrastructure.Data.Repositories;

public class EFCardRepository: ICardRepository
{
    private readonly RapidPayDbContext _dbContext;
    
    public EFCardRepository(RapidPayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void CreateCard(Card card)
    {
        _dbContext.Add(card);
        _dbContext.SaveChanges();
    }

    public Card? GetCard(string cardNumber)
    {
        return _dbContext.Cards.FirstOrDefault(x => x.CardNumber.Equals(cardNumber));
    }

    public void UpdateCard(Card card)
    {
        if (!CardExists(card.CardNumber))
            throw new ArgumentException("Attempted to update card that does not exist.");
        
        _dbContext.Update(card);
        _dbContext.SaveChanges();
    }

    public bool CardExists(string cardNumber)
    {
        return _dbContext.Cards.Any(x => x.CardNumber.Equals(cardNumber));
    }
}