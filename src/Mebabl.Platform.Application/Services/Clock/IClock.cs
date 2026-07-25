namespace Mebabl.Platform.Application.Services.Clock;

public interface IClock
{
    DateTime UtcNow { get; }
}