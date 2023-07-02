using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Entities;
using RapidPay.Core.Interfaces.Infrastructure.Data.Repositories;
using RapidPay.Core.Interfaces.Services;

namespace RapidPay.Core.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICardNumberFactory _cardNumberFactory;

    public CardService(ICardRepository cardRepository, IUserRepository userRepository, ICardNumberFactory cardNumberFactory)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
        _cardNumberFactory = cardNumberFactory;
    }

    public ServiceActionResult<CreateCardResponseDTO> CreateCard(CreateCardRequestDTO request)
    {
        var result = ValidateCreateCardRequest(request);
        
        if (result.Status != ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus.Success)
            return result;

        var card = new Card
        {
            UserId = request.UserID,
            Balance = request.InitialBalance,
            CardNumber = _cardNumberFactory.GenerateCardNumber(),
            Id = Guid.NewGuid()
        };

        _cardRepository.CreateCard(card);
        
        result.ActionResult = new CreateCardResponseDTO
        {
            CardNumber = card.CardNumber,
            InitialBalance = card.Balance,
            UserID = card.UserId
        };

        return result;
    }

    public ServiceActionResult<BalanceResponseDTO> GetBalance(BalanceRequestDTO request)
    {
        var result = ValidateGetBalanceRequest(request);

        if (result.Status != ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success)
            return result;

        result.ActionResult = new BalanceResponseDTO
        {
            Balance = _cardRepository.GetCard(request.CardNumber)!.Balance,
            CardNumber = request.CardNumber
        };

        return result;
    }
    
    private ServiceActionResult<CreateCardResponseDTO> ValidateCreateCardRequest(CreateCardRequestDTO request)
    {
        var result = new ServiceActionResult<CreateCardResponseDTO>();
        result.Status = ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus.Success;

        if (!_userRepository.UserExists(request.UserID))
        {
            result.Status =
                ServiceActionResult<CreateCardResponseDTO>.ServiceActionResultStatus
                    .SecureFailure;
            result.ActionResultMessage = "User does not exist";

            return result;
        }

        return result;
    }

    private ServiceActionResult<BalanceResponseDTO> ValidateGetBalanceRequest(BalanceRequestDTO request)
    {
        var result = new ServiceActionResult<BalanceResponseDTO>();
        result.Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.Success;

        if (!_cardRepository.CardExists(request.CardNumber))
        {
            result.Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = "Card does not exist.";

            return result;
        }

        if (!CardBelongsToUser(request.UserId, request.CardNumber))
        {
            result.Status = ServiceActionResult<BalanceResponseDTO>.ServiceActionResultStatus.SecureFailure;
            result.ActionResultMessage = "Card does not belong to user.";

            return result;
        }

        return result;
    }

    private bool CardBelongsToUser(Guid userId, string cardNumber)
    {
        var card = _cardRepository.GetCard(cardNumber);
        return card!.UserId == userId;
    }
}