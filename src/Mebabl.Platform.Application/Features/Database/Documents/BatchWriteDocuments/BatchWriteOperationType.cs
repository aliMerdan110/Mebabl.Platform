namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// Supported atomic document write operations.
/// </summary>
public enum BatchWriteOperationType
{
    Create = 1,
    Update = 2,
    Delete = 3
}
