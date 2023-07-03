using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

namespace RapidPay.Infrastructure.Data.Repositories;

public class EFCardRepository : ICardRepository
{
    private readonly RapidPayDbContext _dbContext;
    private readonly ConcurrentDictionary<string, Card> _cache = new();

    public EFCardRepository(RapidPayDbContext dbContext)
    {
        _dbContext = dbContext;

        FillCache();
    }

    private void FillCache()
    {
        foreach (var card in _dbContext.Cards)
        {
            _cache[card.CardNumber] = card;
        }
    }

    public async Task CreateCard(Card card)
    {
        _cache[card.CardNumber] = card;

        _dbContext.Add(card);
        await _dbContext.SaveChangesAsync();
    }

    public Card? GetCard(string cardNumber)
    {
        if (!_cache.ContainsKey(cardNumber)) return null;

        return _cache[cardNumber];
    }

    public async Task UpdateCard(Card card)
    {
        if (!CardExists(card.CardNumber))
            throw new ArgumentException("Attempted to update card that does not exist.");

        _cache[card.CardNumber] = card;

        _dbContext.Update(card);
        await _dbContext.SaveChangesAsync();
    }

    public bool CardExists(string cardNumber)
    {
        return _cache.ContainsKey(cardNumber);
    }
}