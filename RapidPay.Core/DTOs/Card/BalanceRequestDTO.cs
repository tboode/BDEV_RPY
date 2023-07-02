namespace RapidPay.Core.DTOs.Card;

public class BalanceRequestDTO
{
    public Guid UserId { get; set; }
    public string CardNumber { get; set; }
}