using RapidPay.Core.Interfaces.Services;

namespace RapidPay.Core.Services;

public class UniversalFeeExchangeService: IUniversalFeeExchangeService
{
    private const int ONE_HOUR_IN_MILLISECONDS = 1000 * 60 * 60;
    
    private readonly ReaderWriterLockSlim _multiplierLock = new();

    private decimal _currentMultiplier;
    
    private readonly Random _random = new();
    
    public UniversalFeeExchangeService()
    {
        UpdateMultiplier();
        
        var feeMultiplierRotator = new Task(StartFeeMultiplierRotator);
        feeMultiplierRotator.Start();
    }
    
    public decimal GetFee(decimal? lastFee)
    {
        if (lastFee == null) return GetCurrentMultiplier();
        
        return lastFee.Value * GetCurrentMultiplier();
    }
    
    private decimal GetCurrentMultiplier()
    {
        _multiplierLock.EnterReadLock();

        try
        {
            return _currentMultiplier;
        }
        finally
        {
            _multiplierLock.ExitReadLock();
        }
    }
    
    private void StartFeeMultiplierRotator()
    {
        while (true)
        {
            Thread.Sleep(ONE_HOUR_IN_MILLISECONDS);
            UpdateMultiplier();
        }
    }
    
    private void UpdateMultiplier()
    {
        _multiplierLock.EnterWriteLock();
        
        _currentMultiplier = (decimal) (_random.NextDouble() * 2);

        _multiplierLock.ExitWriteLock();
    }
}