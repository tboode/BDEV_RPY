using Microsoft.AspNetCore.Mvc;
using RapidPay.Core.DTOs;
using RapidPay.Core.DTOs.Payment;
using RapidPay.Core.Interfaces;
using RapidPay.Core.Interfaces.Services;

namespace RapidPay.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    
    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    
    [HttpPut]
    public IActionResult Pay(PaymentRequestDTO request)
    {
        return this.HandleServiceActionResult(_paymentService.Pay(request, this.ReadUserId()));
    }
}