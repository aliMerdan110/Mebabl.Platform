namespace Mebabl.Platform.Application.Features.SdkSocial.DTOs;

public sealed record PostSocialStatsDto(
    Guid PostId,
    int Likes,
    int Comments,
    int Shares,
    int Reposts,
    bool Liked,
    bool Reposted);