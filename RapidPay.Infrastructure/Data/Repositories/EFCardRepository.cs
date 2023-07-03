using System.Collections.Concurrent;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

namespace RapidPay.Infrastructure.Data.Repositories;

public class EFCardRepository : ICardRepository
{
    private readonly RapidPayDbContext _dbContext;
    private readonly ConcurrentDictionary<string, Card> _cache = new();

    private static readonly ReaderWriterLockSlim _dbLock = new(); // Entity framework with SQLite is not thread safe by default.

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

        await AddToDb(card);
    }

    private Task AddToDb(Card card)
    {
        return Task.Run(() =>
        {
            _dbLock.EnterWriteLock();
            try
            {
                _dbContext.Add(card);
                _dbContext.SaveChanges();
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        });
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
        
        await UpdateInDb(card);
    }

    private Task UpdateInDb(Card card)
    {
        return Task.Run(() =>
        {
            _dbLock.EnterWriteLock();
            try
            {
                _dbContext.Update(card);
                _dbContext.SaveChanges();
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        });
    }

    public bool CardExists(string cardNumber)
    {
        return _cache.ContainsKey(cardNumber);
    }
}