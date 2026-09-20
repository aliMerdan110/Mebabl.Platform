using MediatR;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.DeleteComment;

public sealed record DeleteCommentCommand(
    Guid CommentId) : IRequest;