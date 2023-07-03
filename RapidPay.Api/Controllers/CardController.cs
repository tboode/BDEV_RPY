using System.Net;
using FluentValidation;
using FluentValidation.Results;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using RapidPay.Core.DTOs;
using RapidPay.Core.DTOs.Card;
using RapidPay.Core.Interfaces;
using RapidPay.Core.Interfaces.Services;
using RapidPay.Core.Services;

namespace RapidPay.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly IValidator<CreateCardRequestDTO> _createCardRequestDTOValidator;
    
    public CardController(ICardService cardService, IValidator<CreateCardRequestDTO> createCardRequestDtoValidator)
    {
        _cardService = cardService;
        _createCardRequestDTOValidator = createCardRequestDtoValidator;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateCardRequestDTO request)
    {
        var validationResult = await _createCardRequestDTOValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        return this.HandleServiceActionResult(await _cardService.CreateCard(request, this.ReadUserId()));
    }
    
    [HttpGet]
    public IActionResult GetBalance(string cardNumber)
    {
        return this.HandleServiceActionResult(_cardService.GetBalance(cardNumber, this.ReadUserId()));
    }
}