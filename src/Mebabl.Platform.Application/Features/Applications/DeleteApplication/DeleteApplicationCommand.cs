using MediatR;

namespace Mebabl.Platform.Application.Features.Applications.DeleteApplication;

public sealed record DeleteApplicationCommand(Guid Id) : IRequest;