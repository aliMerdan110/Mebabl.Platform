
using System.Net;
using System.Text.Json;
using Mebabl.Platform.Application.Features.Developers.ResetPassword;

namespace Mebabl.Platform.API.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                exception.Message);

            context.Response.ContentType =
                "application/json";

            var statusCode = exception switch
            {
                PasswordResetTokenInvalidException =>
                    HttpStatusCode.Unauthorized,

                DeveloperAccountInactiveException =>
                    HttpStatusCode.Unauthorized,

                UnauthorizedAccessException =>
                    HttpStatusCode.Unauthorized,

                ArgumentException =>
                    HttpStatusCode.BadRequest,

                KeyNotFoundException =>
                    HttpStatusCode.NotFound,

                _ =>
                    HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode =
                (int)statusCode;

            var response = new
            {
                success = false,

                message = exception is
                    PasswordResetTokenInvalidException
                        or DeveloperAccountInactiveException
                        or UnauthorizedAccessException
                    ? exception.Message
                    : statusCode ==
                        HttpStatusCode.InternalServerError
                        ? "An unexpected error has occurred."
                        : exception.Message,

                statusCode = context.Response.StatusCode
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
