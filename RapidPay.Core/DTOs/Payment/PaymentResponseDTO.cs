namespace RapidPay.Core.DTOs.Payment;

public class PaymentResponseDTO
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Balance { get; set; }
}