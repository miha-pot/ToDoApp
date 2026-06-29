using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ToDoApp.WebAPI.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
                                  IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                Exception exception,
                                                CancellationToken cancellationToken)
    {
        _logger.LogError(exception,
                         "An unhandled exception occurred while processing request on {Path}. " +
                         "Type: {ExceptionType}. Message: {Message}",
                         httpContext.Request.Path,
                         exception.GetType().Name,
                         exception.Message);

        var (statusCode, title, mappedDetail) = exception switch
        {
            OperationCanceledException =>
            (StatusCodes.Status499ClientClosedRequest, "User canceled request.", exception.Message),

            // 1. Podatek ne obstaja v bazi (npr. _context.Todos.Find(id) vrne null in koda vrže izjemo)
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource Not Found", exception.Message),

            // 3. Concurrency napake (ko nekdo spremeni podatek medtem ko si ga ti bral in poskušal shraniti)
            DbUpdateConcurrencyException =>
                (StatusCodes.Status412PreconditionFailed, "Concurrency Error", "The record was modified by another user."),

            // 2. Kršitve unikatnosti ali tujih ključev v EF Core (npr. dvojna registracija istega emaila)
            DbUpdateException =>
                (StatusCodes.Status409Conflict, "Database Conflict", "A database constraint violation occurred."),

            // 4. Splošne bazične napake (npr. ko strežnik baze sploh ni dosegljiv - Timeout)
            DbException =>
                (StatusCodes.Status503ServiceUnavailable, "Database Unavailable", "Database is temporarily unavailable."),

            // 5. Napačna operacija ali kršitev poslovnih pravil znotraj aplikacije
            InvalidOperationException =>
                (StatusCodes.Status400BadRequest, "Invalid Operation", exception.Message),

            // 6. Eksplicitna zavrnitev dostopa v kodi
            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, "Forbidden", "You do not have permission to access this resource."),

            // 7. Vse ostale nepričakovane sistemske napake (NullReferenceException, itd.)
            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        string finalDetail = mappedDetail;

        if (!_env.IsDevelopment() && statusCode == StatusCodes.Status500InternalServerError)
        {
            finalDetail = "A technical error occurred. Please try again later or contact support.";
        }

        // 3. Create the standard ProblemDetails response
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = finalDetail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        // 4. Write the response as JSON
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that this exception has been handled
        return true;
    }
}
