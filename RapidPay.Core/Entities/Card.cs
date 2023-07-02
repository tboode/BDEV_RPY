using RapidPay.Core.Entities.Base;

namespace RapidPay.Core.Entities;

public class Card: EntityBase
{
    public Guid UserId { get; set; }
    public string CardNumber { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal? LastFee { get; set; } = null;
}