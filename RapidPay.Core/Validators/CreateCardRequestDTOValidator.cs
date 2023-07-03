using FluentValidation;
using RapidPay.Core.DTOs;
using RapidPay.Core.DTOs.Card;

namespace RapidPay.Core.Validators;

public class CreateCardRequestDTOValidator: AbstractValidator<CreateCardRequestDTO>
{
    public CreateCardRequestDTOValidator()
    {
        RuleFor(x => x.InitialBalance).
            GreaterThan(0).
            WithMessage("Initial balance must be greater than 0");
    }
}