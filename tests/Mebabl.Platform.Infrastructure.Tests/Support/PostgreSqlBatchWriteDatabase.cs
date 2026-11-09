using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mebabl.Platform.Infrastructure.Tests.Support;

internal sealed class PostgreSqlBatchWriteDatabase : IAsyncDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _schema;
    private readonly DbContextOptions<BatchWriteTestDbContext> _options;

    private PostgreSqlBatchWriteDatabase(
        string adminConnectionString,
        string schema,
        DbContextOptions<BatchWriteTestDbContext> options)
    {
        _adminConnectionString = adminConnectionString;
        _schema = schema;
        _options = options;
    }

    public BatchWriteTestDbContext CreateContext() => new(_options);

    public static async Task<PostgreSqlBatchWriteDatabase> CreateAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString = Environment.GetEnvironmentVariable("MEBABL_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "MEBABL_TEST_POSTGRES must contain a disposable PostgreSQL test connection string.");
        }

        var schema = $"batch_test_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE SCHEMA \"{schema}\";";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        var scopedConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SearchPath = schema
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<BatchWriteTestDbContext>()
            .UseNpgsql(scopedConnectionString)
            .Options;

        var database = new PostgreSqlBatchWriteDatabase(
            connectionString,
            schema,
            options);

        try
        {
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync(cancellationToken);
            return database;
        }
        catch
        {
            await database.DisposeAsync();
            throw;
        }
    }

    public async Task InstallFailureTriggerAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE OR REPLACE FUNCTION reject_batch_write()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $$
            BEGIN
                IF NEW."Key" = 'db-failure' THEN
                    RAISE EXCEPTION 'Forced batch integration-test failure';
                END IF;
                RETURN NEW;
            END;
            $$;

            CREATE TRIGGER trg_reject_batch_write
            BEFORE INSERT OR UPDATE ON "Documents"
            FOR EACH ROW EXECUTE FUNCTION reject_batch_write();
            """,
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE;";
        await command.ExecuteNonQueryAsync();
    }
}
