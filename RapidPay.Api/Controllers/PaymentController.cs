using FluentValidation;
using FluentValidation.Results;
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
    private readonly IValidator<PaymentRequestDTO> _paymentRequestDtoValidator;

    public PaymentController(IPaymentService paymentService, IValidator<PaymentRequestDTO> paymentRequestDtoValidator)
    {
        _paymentService = paymentService;
        _paymentRequestDtoValidator = paymentRequestDtoValidator;
    }
    
    [HttpPut]
    public async Task<IActionResult> Pay(PaymentRequestDTO request)
    {
        ValidationResult validationResult = await _paymentRequestDtoValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        return this.HandleServiceActionResult(await _paymentService.Pay(request, this.ReadUserId()));
    }
}