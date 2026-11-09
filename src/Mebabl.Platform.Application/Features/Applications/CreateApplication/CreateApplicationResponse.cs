namespace Mebabl.Platform.Application.Features.Applications.CreateApplication;

public sealed record CreateApplicationResponse(
    Guid ApplicationId,
    string Name,
    string Code,
    string ApiKey,
    string ApiSecret);