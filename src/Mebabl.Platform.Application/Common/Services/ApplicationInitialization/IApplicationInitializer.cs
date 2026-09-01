namespace Mebabl.Platform.Application.Common.Services.ApplicationInitialization;

public interface IApplicationInitializer
{
    Task InitializeAsync(
        Guid applicationId,
        CancellationToken cancellationToken);
}