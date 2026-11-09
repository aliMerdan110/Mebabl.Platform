using Xunit;

namespace Mebabl.Platform.Infrastructure.Tests.Support;

/// <summary>
/// Runs a test only when an explicitly isolated PostgreSQL test connection
/// has been supplied. This prevents accidental use of development/production
/// connection strings.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("MEBABL_TEST_POSTGRES")))
        {
            Skip = "Set MEBABL_TEST_POSTGRES to a disposable PostgreSQL test database to run this integration test.";
        }
    }
}
