using Microsoft.EntityFrameworkCore;

namespace Mebabl.Platform.Infrastructure.Tests.Support;

/// <summary>
/// Creates a PostgreSQL-provider context for SQL-generation and model tests.
/// ToQueryString() and model metadata inspection do not open a connection.
/// </summary>
internal static class PostgreSqlTestContextFactory
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=mebabl_query_engine_tests;Username=postgres;Password=postgres";

    public static QueryEngineTestDbContext Create()
    {
        var options = new DbContextOptionsBuilder<QueryEngineTestDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new QueryEngineTestDbContext(options);
    }
}
