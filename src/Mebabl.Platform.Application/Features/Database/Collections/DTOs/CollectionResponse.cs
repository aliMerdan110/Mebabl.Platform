namespace Mebabl.Platform.Application.Features.Database.Collections.DTOs;

public sealed record CollectionResponse(
    Guid Id,
    Guid ApplicationId,
    string Name,
    string Description,
    bool IsActive);