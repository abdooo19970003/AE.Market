using AE.Market.Domain.Common.Abstracts;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Helpers
{
    public static class ResultMapper
    {
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                return new OkResult();

            return CreateProblemResult(result.Error, result.Errors);
        }

        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            return CreateProblemResult(result.Error, result.Errors);
        }

        public static IActionResult ToCreatedActionResult<T>(this Result<T> result, string? location = null)
        {
            if (result.IsSuccess)
                return new CreatedResult(location ?? string.Empty, result.Value);

            return CreateProblemResult(result.Error, result.Errors);
        }

        public static IActionResult ToNotFoundActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            return CreateProblemResult(result.Error, result.Errors);
        }

        public static IActionResult ToDeletedActionResult(this Result result)
        {
            if (result.IsSuccess)
                return new NoContentResult();
            return CreateProblemResult(result.Error, result.Errors);
        }

        private static IActionResult CreateProblemResult(Error error, IEnumerable<Error>? errors)
        {
            var statusCode = MapErrorToStatusCode(error);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitleForStatusCode(statusCode),
                Detail = error.Message,
                Type = $"https://httpstatuses.com/{statusCode}",
                Extensions = { ["error"] = new { error.Code, error.Message } }
            };

            if (errors is not null)
            {
                var errorList = errors.ToList();
                if (errorList.Count != 0)
                {
                    problemDetails.Extensions["errors"] = errorList.Select(e => new { e.Code, e.Message });
                }
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
        }

        private static int MapErrorToStatusCode(Error error)
        {
            if (error.Code.Contains("InvalidCredentials", StringComparison.OrdinalIgnoreCase) ||
                error.Code.Contains("ReplayAttack", StringComparison.OrdinalIgnoreCase) ||
                error.Code.Contains("ExpiredOrRevoked", StringComparison.OrdinalIgnoreCase))
                return StatusCodes.Status401Unauthorized;
            if (error.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
                return StatusCodes.Status404NotFound;
            if (error.Code.Contains("AlreadyExist", StringComparison.OrdinalIgnoreCase) ||
                error.Code.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
                return StatusCodes.Status409Conflict;
            if (error.Code.StartsWith("Application.Validation", StringComparison.OrdinalIgnoreCase))
                return StatusCodes.Status400BadRequest;
            if (error.Code.StartsWith("Application.", StringComparison.OrdinalIgnoreCase))
                return StatusCodes.Status500InternalServerError;

            return StatusCodes.Status400BadRequest;
        }

        private static string GetTitleForStatusCode(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
    }


}
