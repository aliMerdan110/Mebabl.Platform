using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.UpdateComment;

public sealed record UpdateCommentCommand(
    Guid CommentId,
    string Text) : IRequest<CommentDto>;