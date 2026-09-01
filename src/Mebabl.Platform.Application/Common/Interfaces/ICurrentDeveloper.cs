namespace Mebabl.Platform.Application.Common.Interfaces;

public interface ICurrentDeveloper
{
    Guid DeveloperId { get; }

    bool IsAuthenticated { get; }
}