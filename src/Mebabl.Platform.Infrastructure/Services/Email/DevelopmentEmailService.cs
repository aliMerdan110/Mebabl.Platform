using Microsoft.Extensions.Logging;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Services.Email;

public sealed class DevelopmentEmailService
    : IEmailService
{
    private readonly ILogger<DevelopmentEmailService> _logger;

    public DevelopmentEmailService(
        ILogger<DevelopmentEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            """
            ===== DEVELOPMENT EMAIL =====
            To: {To}
            Subject: {Subject}

            {Body}

            =============================
            """,
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}