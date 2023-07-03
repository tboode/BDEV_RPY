using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;

namespace RapidPay.Core.Services.Utils;

public class CardNumberUtils: ICardNumberUtils
{
    private readonly ICardRepository _cardRepository;

    public CardNumberUtils(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public string GenerateCardNumber()
    {
        string result;
        Random random = new Random();

        do
        {
            result = "";

            for (int i = 0; i < 15; i++)
            {
                result += $"{random.Next(0, 9)}";
            }
        } while (_cardRepository.CardExists(result));

        return result;
    }

    public static bool IsValidCardNumber(string cardNumber)
    {
        return cardNumber.All(char.IsDigit) && cardNumber.Length == 15;
    }

    public static string MaskCardNumber(string cardNumber)
    {
        return $"{cardNumber.Substring(0, 4)} **** **** {cardNumber.Substring(12, 3)}";
    }
}