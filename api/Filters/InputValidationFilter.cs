using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Planera.Filters;

public class InputValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        context.Result = new JsonResult(context.ModelState)
        {
            StatusCode = 400,
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}