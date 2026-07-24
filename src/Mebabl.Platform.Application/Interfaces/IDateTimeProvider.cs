namespace Mebabl.Platform.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}