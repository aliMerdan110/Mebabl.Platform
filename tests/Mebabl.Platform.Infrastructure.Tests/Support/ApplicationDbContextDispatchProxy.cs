using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Infrastructure.Tests.Support;

/// <summary>
/// Exposes only the IApplicationDbContext members used by the batch handler,
/// backed by the real PostgreSQL test DbContext.
/// </summary>
public class ApplicationDbContextDispatchProxy : DispatchProxy
{
    private DbContext? _context;

    public void Initialize(DbContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var context = _context
            ?? throw new InvalidOperationException("The test DbContext proxy is not initialized.");
        var cancellationToken = args is { Length: > 0 } && args[0] is CancellationToken token
            ? token
            : CancellationToken.None;

        return targetMethod?.Name switch
        {
            "get_Collections" => context.Set<Collection>(),
            "get_Documents" => context.Set<Document>(),
            "BeginTransactionAsync" => context.Database.BeginTransactionAsync(cancellationToken),
            "SaveChangesAsync" => context.SaveChangesAsync(cancellationToken),
            _ => throw new NotSupportedException(
                $"IApplicationDbContext member '{targetMethod?.Name}' is not configured in this test proxy.")
        };
    }
}
