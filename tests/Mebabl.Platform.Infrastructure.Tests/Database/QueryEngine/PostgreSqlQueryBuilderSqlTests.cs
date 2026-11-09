using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Features.Database.Query;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Infrastructure.Database.QueryEngine;
using Mebabl.Platform.Infrastructure.Tests.Support;
using Xunit;

namespace Mebabl.Platform.Infrastructure.Tests.Database.QueryEngine;

public sealed class PostgreSqlQueryBuilderSqlTests
{
    private static readonly Guid ProjectId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid CollectionId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly PostgreSqlQueryBuilder _builder = new();

    [Fact]
    public void Apply_PreservesProjectAndCollectionScope_AndTranslatesNestedJsonFilter()
    {
        using var context = PostgreSqlTestContextFactory.Create();

        // The query builder must preserve the project/collection/deletion
        // boundary established by the caller; it must never replace this scope.
        var scopedQuery = context.Documents.Where(document =>
            document.ProjectId == ProjectId &&
            document.CollectionId == CollectionId &&
            !document.IsDeleted);

        var request = new QueryRequest
        {
            CollectionId = CollectionId,
            Filters =
            [
                new QueryFilter("profile.status", QueryOperator.Equal, "active")
            ],
            Sorts = [QuerySort.Desc("version")],
            Offset = 10,
            Limit = 25
        };

        var sql = _builder.Apply(scopedQuery, request).ToQueryString();
        var normalizedSql = sql.ToLowerInvariant();

        Assert.Contains("\"projectid\"", normalizedSql);
        Assert.Contains("\"collectionid\"", normalizedSql);
        Assert.Contains("\"isdeleted\"", normalizedSql);
        Assert.Contains("jsonb_typeof", normalizedSql);
        Assert.Contains("\"data\"", normalizedSql);
        Assert.Contains("order by", normalizedSql);
        Assert.Contains("\"version\"", normalizedSql);
        Assert.Contains("limit", normalizedSql);
        Assert.Contains("offset", normalizedSql);
    }

    [Fact]
    public void Apply_TranslatesNumericComparison()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = CreateRequest(
            new QueryFilter("age", QueryOperator.GreaterThanOrEqual, 18));

        var sql = _builder.Apply(context.Documents, request).ToQueryString();
        var normalizedSql = sql.ToLowerInvariant();

        Assert.Contains("jsonb_typeof", normalizedSql);
        Assert.Contains("age", normalizedSql);
        Assert.Contains(">=", normalizedSql);
    }

    [Fact]
    public void Apply_TranslatesBooleanComparison()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = CreateRequest(
            new QueryFilter("isPublished", QueryOperator.Equal, true));

        var sql = _builder.Apply(context.Documents, request).ToQueryString();

        Assert.Contains("jsonb_typeof", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isPublished", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_TranslatesJsonNullComparison()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = CreateRequest(
            new QueryFilter("optionalValue", QueryOperator.Equal, null));

        var sql = _builder.Apply(context.Documents, request).ToQueryString();

        Assert.Contains("jsonb_typeof", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("optionalValue", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_TranslatesStringSearchAndDeterministicDefaultSort()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = new QueryRequest
        {
            CollectionId = CollectionId,
            Search = "invoice",
            Limit = 20
        };

        var sql = _builder.Apply(context.Documents, request).ToQueryString();
        var normalizedSql = sql.ToLowerInvariant();

        Assert.Contains("like", normalizedSql);
        Assert.Contains("order by", normalizedSql);
        Assert.Contains("\"createdat\"", normalizedSql);
        Assert.Contains("\"id\"", normalizedSql);
    }

    [Fact]
    public void Apply_RejectsInvalidJsonFieldPath()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = CreateRequest(
            new QueryFilter("profile..status", QueryOperator.Equal, "active"));

        Assert.Throws<ArgumentException>(() =>
        {
            _builder.Apply(context.Documents, request);
        });
    }

    [Fact]
    public void Apply_RejectsArrayFilterValues()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = CreateRequest(
            new QueryFilter("tags", QueryOperator.Equal, new[] { "one", "two" }));

        Assert.Throws<ArgumentException>(() =>
        {
            _builder.Apply(context.Documents, request);
        });
    }

    [Fact]
    public void Apply_RejectsUnsupportedProjection()
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = new QueryRequest
        {
            CollectionId = CollectionId,
            Select = ["name"]
        };

        Assert.Throws<ArgumentException>(() =>
        {
            _builder.Apply(context.Documents, request);
        });
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(100001, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 102)]
    public void Apply_RejectsOutOfRangePaging(int offset, int limit)
    {
        using var context = PostgreSqlTestContextFactory.Create();
        var request = new QueryRequest
        {
            CollectionId = CollectionId,
            Offset = offset,
            Limit = limit
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _builder.Apply(context.Documents, request);
        });
    }

    private static QueryRequest CreateRequest(QueryFilter filter) => new()
    {
        CollectionId = CollectionId,
        Filters = [filter],
        Limit = 25
    };
}
