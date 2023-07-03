using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Services;

namespace RapidPay.Core.Interfaces.Services;

public interface IPaymentService
{
    Task<ServiceActionResult<PaymentResponseDTO>> Pay(PaymentRequestDTO request, string userSubjectId);
}