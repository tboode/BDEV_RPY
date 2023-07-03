namespace RapidPay.Core.DTOs.Card;

public class BalanceResponseDTO
{
    public string CardNumber { get; set; } = default!;
    public decimal Balance { get; set; }
}