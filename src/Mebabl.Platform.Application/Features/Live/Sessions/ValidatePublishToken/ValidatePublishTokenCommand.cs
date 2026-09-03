// Application/Features/Live/Sessions/ValidatePublishToken/ValidatePublishTokenCommand.cs

using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Sessions.ValidatePublishToken;

public sealed record ValidatePublishTokenCommand(
    Guid SessionId,
    string PublishToken
) : IRequest<bool>;