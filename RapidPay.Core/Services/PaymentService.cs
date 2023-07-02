using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;

namespace RapidPay.Core.Services;

public class PaymentService : IPaymentService
{
    private readonly ICardRepository _cardRepository;
    private readonly IUniversalFeeExchangeService _universalFeeExchangeService;

    public PaymentService(ICardRepository cardRepository, IUniversalFeeExchangeService universalFeeExchangeService)
    {
        _cardRepository = cardRepository;
        _universalFeeExchangeService = universalFeeExchangeService;
    }

    public ServiceActionResult<PaymentResponseDTO> Pay(PaymentRequestDTO request)
    {
        var result = ValidatePaymentRequest(request);

        // If the status is not Success, return the validation result
        if (result.Status != ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Success)
        {
            return result;
        }

        var card = _cardRepository.GetCard(request.CardNumber);

        var fee = _universalFeeExchangeService.GetFee(card.LastFee);
        
        card.Balance -= request.Amount + fee;
        card.LastFee = fee;
        
        _cardRepository.UpdateCard(card);

        result.ActionResult = new PaymentResponseDTO
        {
            CardNumber = card.CardNumber,
            Amount = request.Amount,
            Fee = fee,
            Balance = card.Balance,
            TotalAmount = request.Amount + fee
        };

        return result;
    }

    private ServiceActionResult<PaymentResponseDTO> ValidatePaymentRequest(PaymentRequestDTO request)
    {
        var result = new ServiceActionResult<PaymentResponseDTO>();

        // Verify card exists.
        if (!_cardRepository.CardExists(request.CardNumber))
        {
            result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = "Card does not exist.";

            return result;
        }

        var card = _cardRepository.GetCard(request.CardNumber);

        // Verify card belongs to user.
        if (!card.UserId.Equals(request.UserId))
        {
            result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = "Card does not belong to user.";

            return result;
        }

        // Verify card has sufficient funds.
        var fee = _universalFeeExchangeService.GetFee(card.LastFee);

        if (card.Balance < request.Amount + fee)
        {
            result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Failure;
            result.ActionResultMessage = "Insufficient funds.";

            return result;
        }

        // If all checks pass, return a successful result
        result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Success;
        return result;
    }
}