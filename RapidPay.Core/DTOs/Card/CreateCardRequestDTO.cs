namespace RapidPay.Core.DTOs.Card;

public class CreateCardRequestDTO
{
    public Guid UserID { get; set; }
    public decimal InitialBalance { get; set; }
}