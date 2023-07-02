using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Services;

namespace RapidPay.Core.Interfaces.Services;

public interface ICardService
{
    ServiceActionResult<CreateCardResponseDTO> CreateCard(CreateCardRequestDTO request);
    ServiceActionResult<BalanceResponseDTO> GetBalance(BalanceRequestDTO request);
}