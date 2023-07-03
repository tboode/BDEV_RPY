using FluentValidation;
using RapidPay.Core.DTOs;
using RapidPay.Core.DTOs.Payment;

namespace RapidPay.Core.Validators;

public class PaymentRequestDTOValidator: AbstractValidator<PaymentRequestDTO>
{  
    public PaymentRequestDTOValidator()
    {
        RuleFor(x => x.CardNumber).NotEmpty();
        RuleFor(x => x.CardNumber).Length(15);
        RuleFor(x => x.CardNumber).Matches(@"^\d{15}$");
        
        RuleFor(x => x.Amount).
            GreaterThan(0).
            WithMessage("Amount must be greater than 0");
    }
}