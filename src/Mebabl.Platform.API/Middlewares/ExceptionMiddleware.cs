using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
            if (context.Response.HasStarted)
                throw;

            var statusCode = exception switch
            {
                Mebabl.Platform.Application.Features.SdkAuth.ResetPassword
                    .PasswordResetTokenInvalidException =>
                    HttpStatusCode.BadRequest,

                PasswordResetTokenInvalidException =>
                    HttpStatusCode.Unauthorized,

                DeveloperAccountInactiveException =>
                    HttpStatusCode.Unauthorized,

                UnauthorizedAccessException =>
                    HttpStatusCode.Unauthorized,

                KeyNotFoundException =>
                    HttpStatusCode.NotFound,

                ArgumentException =>
                    HttpStatusCode.BadRequest,

                InvalidOperationException =>
                    HttpStatusCode.Conflict,

                DbUpdateException =>
                    HttpStatusCode.Conflict,

                _ =>
                    HttpStatusCode.InternalServerError
            };

            if (exception is DbUpdateException ||
                statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    "Request rejected with status {StatusCode} for {Method} {Path}: {Message}",
                    (int)statusCode,
                    context.Request.Method,
                    context.Request.Path,
                    exception.Message);
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var responseMessage = exception switch
            {
                Mebabl.Platform.Application.Features.SdkAuth.ResetPassword
                    .PasswordResetTokenInvalidException =>
                    "The password reset link is invalid or has expired.",

                _ when exception is not DbUpdateException &&
                       statusCode != HttpStatusCode.InternalServerError =>
                    exception.Message,

                _ =>
                    "An unexpected error has occurred."
            };

            var response = new
            {
                success = false,
                message = responseMessage,
                statusCode = context.Response.StatusCode
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}