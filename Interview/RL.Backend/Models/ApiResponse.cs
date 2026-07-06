using MediatR;
using Microsoft.AspNetCore.Mvc;
using RL.Backend.Exceptions;

namespace RL.Backend.Models;

public class ApiResponse<T> where T : new()
{
    public bool Succeeded => Exception is null;
    public Exception? Exception { get; set; }
    public T? Value { get; set; }

    public static ApiResponse<T> Fail(Exception e)
    {
        return new ApiResponse<T>
        {
            Exception = e
        };
    }

    public static ApiResponse<T> Succeed(T value)
    {
        return new ApiResponse<T>
        {
            Value = value
        };
    }
}

public static class ApiResponseExtensions
{
    public static IActionResult ToActionResult<T>(this ApiResponse<T> response) where T : new()
    {
        if (!response.Succeeded)
        {
            return response.Exception switch
            {
                BadRequestException => new BadRequestObjectResult(response.Exception?.Message),
                NotFoundException => new NotFoundObjectResult(response.Exception?.Message),
                _ => new ObjectResult(response.Exception?.Message) { StatusCode = StatusCodes.Status500InternalServerError }
            };
        }

        if (typeof(T) == typeof(Unit) || response.Value is null)
            return new OkObjectResult(new { success = true });

        return new OkObjectResult(response.Value);
    }
}