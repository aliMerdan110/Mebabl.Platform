namespace Mebabl.Platform.Application.Services.FileStorage;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string url,
        CancellationToken cancellationToken = default);
}