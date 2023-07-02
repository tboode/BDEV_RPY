namespace RapidPay.Core.DTOs.Payment;

public class PaymentRequestDTO
{
    public Guid UserId { get; set; }
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
}