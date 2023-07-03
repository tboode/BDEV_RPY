namespace RapidPay.Core.DTOs.Payment;

public class PaymentRequestDTO
{
    public string CardNumber { get; set; } = default!;
    public decimal Amount { get; set; }
}