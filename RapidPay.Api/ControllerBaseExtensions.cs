using Microsoft.AspNetCore.Mvc;
using RapidPay.Core.Services;

namespace RapidPay.Api;

public static class ControllerBaseExtensions
{
    public static IActionResult HandleServiceActionResult<T>(this ControllerBase controller, ServiceActionResult<T> result)
    {
        if (result.Status == ServiceActionResult<T>.ServiceActionResultStatus.SecureFailure)
            return controller.StatusCode(500);
        
        if (result.Status == ServiceActionResult<T>.ServiceActionResultStatus.Failure)
            return controller.BadRequest(result.ActionResultMessage);

        return controller.Ok(result.ActionResult);
    }

    public static string ReadUserId(this ControllerBase controller)
    {
        return controller.User.Claims.
            First(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"))
            .Value;
    }
}