using Mebabl.Platform.Application.Services.Clock;

namespace Mebabl.Platform.Infrastructure.Services.Clock;

public sealed class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}