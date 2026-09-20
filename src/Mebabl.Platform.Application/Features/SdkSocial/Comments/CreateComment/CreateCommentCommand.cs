using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.CreateComment;

public sealed record CreateCommentCommand(
    Guid PostId,
    string Text) : IRequest<CommentDto>;