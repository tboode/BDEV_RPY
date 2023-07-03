using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Services;

namespace RapidPay.Core.Interfaces.Services;

public interface ICardService
{
    Task<ServiceActionResult<CreateCardResponseDTO>> CreateCard(CreateCardRequestDTO request, string userSubjectId);
    ServiceActionResult<BalanceResponseDTO> GetBalance(string cardNumber, string userSubjectId);
}