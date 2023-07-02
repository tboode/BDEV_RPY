namespace RapidPay.Core.Services;

public class ServiceActionResult<T>
{
    public enum ServiceActionResultStatus
    {
        Success,
        Failure,
        SecureFailure
    }
    
    public T? ActionResult { get; set; }
    public string? ActionResultMessage;
    public ServiceActionResultStatus Status { get; set; }
}