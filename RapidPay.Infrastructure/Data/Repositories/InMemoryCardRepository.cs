using System.Collections.Concurrent;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

namespace RapidPay.Infrastructure.Data.Repositories;

public class InMemoryCardRepository : ICardRepository
{
    private readonly ConcurrentDictionary<string, Card> _cards = new();
    
    public void CreateCard(Card card)
    {
        if (_cards.ContainsKey(card.CardNumber))
            throw new ArgumentException("Attempted to create card that already exists.");
        
        _cards[card.CardNumber] = card;
    }

    public Card? GetCard(string cardNumber)
    {
        return !_cards.ContainsKey(cardNumber) ? null : _cards[cardNumber];
    }

    public void UpdateCard(Card card)
    {
        if (!_cards.ContainsKey(card.CardNumber))
            throw new ArgumentException("Attempted to update card that does not exist.");
        
        _cards[card.CardNumber] = card;
    }
    
    public bool CardExists(string cardNumber)
    {
        return _cards.ContainsKey(cardNumber);
    }
}