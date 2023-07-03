using Microsoft.Extensions.Logging;
using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;
using RapidPay.Core.Services.Utils;

namespace RapidPay.Core.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly ICardNumberUtils _cardNumberUtils;
    private readonly ILogger<CardService> _logger;

    public CardService(ICardRepository cardRepository, ICardNumberUtils cardNumberUtils, ILogger<CardService> logger)
    {
        _cardRepository = cardRepository;
        _cardNumberUtils = cardNumberUtils;
        _logger = logger;
    }

    public async Task<ServiceActionResult<CreateCardResponseDTO>> CreateCard(CreateCardRequestDTO request, string userSubjectId)
    {
        _logger.Log(LogLevel.Information, "Creating new card for user {UserSubjectId}", userSubjectId);

        var result = new ServiceActionResult<CreateCardResponseDTO>();

        var card = new Card
        {
            UserId = userSubjectId,
            Balance = request.InitialBalance,
            CardNumber = _cardNumberUtils.GenerateCardNumber(),
            Id = Guid.NewGuid()
        };

        await _cardRepository.CreateCard(card);

        _logger.Log(LogLevel.Information, "Created new card for user {UserSubjectId} successfully", userSubjectId);

        result.Status = ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus.Success;
        result.ActionResult = new CreateCardResponseDTO
        {
            CardNumber = card.CardNumber,
            InitialBalance = card.Balance
        };

        return result;
    }

    public ServiceActionResult<BalanceResponseDTO> GetBalance(string cardNumber, string userSubjectId)
    {
        var result = ValidateGetBalanceRequest(cardNumber, userSubjectId);

        if (result.Status != ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success)
        {
            _logger.Log(
                LogLevel.Warning, 
                "Validation failed for get balance request for user {UserSubjectId}. Reason: {ResultActionResultMessage}", 
                userSubjectId, 
                result.ActionResultMessage);

            return result;
        }

        result.ActionResult = new BalanceResponseDTO
        {
            Balance = _cardRepository.GetCard(cardNumber)!.Balance,
            CardNumber = cardNumber
        };

        return result;
    }

    private ServiceActionResult<BalanceResponseDTO> ValidateGetBalanceRequest(string cardNumber, string userSubjectId)
    {
        var result = new ServiceActionResult<BalanceResponseDTO>
        {
            Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success
        };

        if (!CardNumberUtils.IsValidCardNumber(cardNumber))
        {
            result.Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Failure;
            result.ActionResultMessage = "Card number is not valid.";

            return result;
        }

        if (!_cardRepository.CardExists(cardNumber))
        {
            result.Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = $"Card {CardNumberUtils.MaskCardNumber(cardNumber)} does not exist.";

            return result;
        }

        if (!CardBelongsToUser(userSubjectId, cardNumber))
        {
            result.Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = $"Card {CardNumberUtils.MaskCardNumber(cardNumber)} does not belong to user.";

            return result;
        }

        return result;
    }

    private bool CardBelongsToUser(string userSubjectId, string cardNumber)
    {
        var card = _cardRepository.GetCard(cardNumber);
        return card!.UserId.Equals(userSubjectId);
    }
}