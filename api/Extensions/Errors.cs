using Microsoft.AspNetCore.Mvc.ModelBinding;
using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Planera.Extensions;

public static class Errors
{
    public static IActionResult ToActionResult<T>(this ErrorOr<T> errors)
    {
        if (errors.IsError)
            return new BadRequestObjectResult(errors.ToModelState());

        return new OkObjectResult(errors.Value);
    }

    public static ModelStateDictionary ToModelState<T>(this ErrorOr<T> errors, ModelStateDictionary? preexisting = null)
    {
        var dictionary = preexisting ?? new ModelStateDictionary();
        foreach (var error in errors.Errors)
            dictionary.AddModelError(error.Code, error.Description);

        return dictionary;
    }
}