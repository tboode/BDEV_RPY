using Microsoft.Extensions.Logging;
using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;
using RapidPay.Core.Services.Utils;

namespace RapidPay.Core.Services;

public class PaymentService : IPaymentService
{
    private readonly ICardRepository _cardRepository;
    private readonly IUniversalFeeExchangeService _universalFeeExchangeService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ICardRepository cardRepository, IUniversalFeeExchangeService universalFeeExchangeService, ILogger<PaymentService> logger)
    {
        _cardRepository = cardRepository;
        _universalFeeExchangeService = universalFeeExchangeService;
        _logger = logger;
    }

    public async Task<ServiceActionResult<PaymentResponseDTO>> Pay(PaymentRequestDTO request, string userSubjectId)
    {
        var maskedCardNumber = CardNumberUtils.MaskCardNumber(request.CardNumber);

        _logger.Log(
            LogLevel.Information, 
            "Processing payment of {RequestAmount} on card {MaskedCardNumber} for user {UserSubjectId}", 
            request.Amount, 
            maskedCardNumber, 
            userSubjectId);

        var result = ValidatePaymentRequest(request, userSubjectId);

        if (result.Status != ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Success)
        {
            _logger.Log(
                LogLevel.Warning, 
                "Validation failed for payment request for card {MaskedCardNumber} for user {UserSubjectId}. Reason: {ResultActionResultMessage}", 
                maskedCardNumber, 
                userSubjectId, 
                result.ActionResultMessage);
            
            return result;
        }

        var card = _cardRepository.GetCard(request.CardNumber);

        var fee = _universalFeeExchangeService.GetFee(card!.LastFee);
        
        card.Balance -= request.Amount + fee;
        card.LastFee = fee;
        
        await _cardRepository.UpdateCard(card);

        _logger.Log(
            LogLevel.Information, 
            "Processed payment of {RequestAmount} on card {MaskedCardNumber} for user {UserSubjectId} successfully", 
            request.Amount, 
            maskedCardNumber, 
            userSubjectId);

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

    private ServiceActionResult<PaymentResponseDTO> ValidatePaymentRequest(PaymentRequestDTO request, string userSubjectId)
    {
        var result = new ServiceActionResult<PaymentResponseDTO>
        {
            Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Success
        };

        if (!_cardRepository.CardExists(request.CardNumber))
        {
            result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = "Card does not exist.";

            return result;
        }

        var card = _cardRepository.GetCard(request.CardNumber);

        if (!card!.UserId.Equals(userSubjectId))
        {
            result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = "Card does not belong to user.";

            return result;
        }

        var fee = _universalFeeExchangeService.GetFee(card.LastFee);

        if (card.Balance < request.Amount + fee)
        {
            result.Status = ServiceActionResult<PaymentResponseDTO>.ServiceActionResultStatus.Failure;
            result.ActionResultMessage = "Insufficient funds.";

            return result;
        }

        return result;
    }
}