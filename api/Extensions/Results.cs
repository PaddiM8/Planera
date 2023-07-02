using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Planera.Extensions;

public static class Errors
{
    public static IActionResult ToActionResult<T>(this ErrorOr<T> errors)
    {
        if (errors.IsError)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Invalid request.",
                Status = 400,
            };
            problemDetails.Extensions.Add("errors", FormatErrors(errors.Errors));

            return new BadRequestObjectResult(problemDetails);
        }

        return new OkObjectResult(errors.Value);
    }

    private static Dictionary<string, List<string>> FormatErrors(this List<Error> errors)
    {
        var dictionary = new Dictionary<string, List<string>>();
        foreach (var error in errors)
        {
            var (field, _) = ParseErrorCode(error.Code);
            string key = field ?? "*";
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new List<string>());

            dictionary[key].Add(error.Description);
        }

        return dictionary;
    }

    private static (string? field, string error) ParseErrorCode(string errorCode)
    {
        var parts = errorCode.Split('.');

        return parts.Length == 1
            ? (null, parts[0])
            : (parts[0], parts[1]);
    }
}