using System.Net;
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
    private readonly ILogger<CardController> _logger;
    private readonly ICardService _cardService;
    
    public CardController(ILogger<CardController> logger, ICardService cardService)
    {
        _logger = logger;
        _cardService = cardService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateCardRequestDTO request)
    {
        return this.HandleServiceActionResult(await _cardService.CreateCard(request, this.ReadUserId()));
    }
    
    [HttpGet]
    public IActionResult GetBalance(string cardNumber)
    {
        return this.HandleServiceActionResult(_cardService.GetBalance(cardNumber, this.ReadUserId()));
    }
}