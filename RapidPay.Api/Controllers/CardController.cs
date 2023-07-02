using System.Net;
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
    public IActionResult Create(CreateCardRequestDTO request)
    {
        return this.HandleServiceActionResult(_cardService.CreateCard(request));
    }
    
    [HttpGet]
    public IActionResult GetBalance(BalanceRequestDTO balanceRequest)
    {
        return this.HandleServiceActionResult(_cardService.GetBalance(balanceRequest));
    }
}