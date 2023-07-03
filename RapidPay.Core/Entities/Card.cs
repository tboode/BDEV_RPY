using RapidPay.Core.Entities.Base;

namespace RapidPay.Core.Entities;

public class Card: EntityBase
{
    public string UserId { get; set; } = default!;
    public string CardNumber { get; set; } = default!;
    public decimal Balance { get; set; } = 0;
    public decimal? LastFee { get; set; } = null;
}