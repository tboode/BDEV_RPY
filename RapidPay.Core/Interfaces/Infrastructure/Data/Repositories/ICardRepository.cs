using RapidPay.Core.Entities;

namespace RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;

public interface ICardRepository
{
    void CreateCard(Card card);
    Card? GetCard(string cardNumber);
    void UpdateCard(Card card);
    bool CardExists(string cardNumber);
}