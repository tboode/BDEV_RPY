using FluentValidation;
using RapidPay.Core.DTOs.Card;

namespace RapidPay.Core.Validators;

public class BalanceRequestDTOValidator: AbstractValidator<BalanceRequestDTO>
{
    public BalanceRequestDTOValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CardNumber).NotEmpty();
        RuleFor(x => x.CardNumber).Length(15);
        RuleFor(x => x.CardNumber).Matches(@"^\d{15}$");
    }
}