using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Services;

namespace RapidPay.Core.Interfaces.Services;

public interface IPaymentService
{
    ServiceActionResult<PaymentResponseDTO> Pay(PaymentRequestDTO request, string userSubjectId);
}