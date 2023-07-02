using Microsoft.AspNetCore.Mvc;
using RapidPay.Core.Services;

namespace RapidPay.Api;

public static class ControllerBaseExtensions
{
    public static IActionResult HandleServiceActionResult<T>(this ControllerBase controller, ServiceActionResult<T> result)
    {
        if (result.Status == ServiceActionResult<T>.ServiceActionResultStatus.SecureFailure)
        {
            return controller.StatusCode(500);
        }
        else if (result.Status == ServiceActionResult<T>.ServiceActionResultStatus.Failure)
        {
            return controller.BadRequest(result.ActionResultMessage);
        }

        return controller.Ok(result.ActionResult);
    }
}