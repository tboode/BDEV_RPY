using RapidPay.Core.Entities;

namespace RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

public interface ICardRepository
{
    Task CreateCard(Card card);
    Card? GetCard(string cardNumber);
    Task UpdateCard(Card card);
    bool CardExists(string cardNumber);
}