namespace RapidPay.Core.Interfaces.Services;

public interface IUniversalFeeExchangeService
{
    decimal GetFee(decimal? lastFee);
}