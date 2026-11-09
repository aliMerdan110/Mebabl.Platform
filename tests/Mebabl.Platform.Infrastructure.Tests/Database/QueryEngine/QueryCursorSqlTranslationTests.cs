using Xunit;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Infrastructure.Database.QueryEngine;
using Mebabl.Platform.Infrastructure.Tests.Support;

namespace Mebabl.Platform.Infrastructure.Tests.Database.QueryEngine;

public sealed class QueryCursorSqlTranslationTests
{
    [Fact]
    public void CursorRowComparison_TranslatesToPostgreSqlRowValueComparison()
    {
        using var db = PostgreSqlTestContextFactory.Create();

        var cursorCreatedAt = new DateTime(
            2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cursorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var sql = db.Documents
            .Where(document => EF.Functions.LessThan(
                ValueTuple.Create(document.CreatedAt, document.Id),
                ValueTuple.Create(cursorCreatedAt, cursorId)))
            .OrderByDescending(document => document.CreatedAt)
            .ThenByDescending(document => document.Id)
            .Take(11)
            .ToQueryString();

        Assert.Contains(" < ", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Matches(
            @"(?im)^--\s+@\w+='11'\s*$",
            sql);
        Assert.Matches(
            @"(?i)LIMIT\s+@\w+",
            sql);
    }

    [Fact]
    public void QueryBuilder_AllowsOneInternalLookAheadRow()
    {
        using var db = PostgreSqlTestContextFactory.Create();
        var builder = new PostgreSqlQueryBuilder();

        var query = builder.Apply(
            db.Documents,
            new QueryRequest
            {
                CollectionId = Guid.NewGuid(),
                Limit = QueryRequest.MaximumInternalPageSize
            });

        var sql = query.ToQueryString();

        Assert.Matches(
            $@"(?im)^--\s+@\w+='{QueryRequest.MaximumInternalPageSize}'\s*$",
            sql);
        Assert.Matches(
            @"(?i)LIMIT\s+@\w+",
            sql);
    }
}
