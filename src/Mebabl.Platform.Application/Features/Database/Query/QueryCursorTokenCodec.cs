using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.Query;

/// <summary>
/// Encodes and validates a versioned cursor position. The token is bound to
/// the authenticated project, collection, and normalized query fingerprint.
/// It is a paging continuation value, never an authorization credential.
/// </summary>
internal static class QueryCursorTokenCodec
{
    private const int CurrentVersion = 1;
    private const int MaximumDecodedTokenBytes = 3072;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 4
    };

    public static string Encode(
        Guid projectId,
        Guid collectionId,
        DateTime createdAt,
        Guid documentId,
        string queryFingerprint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queryFingerprint);

        var payload = new CursorPayload(
            CurrentVersion,
            projectId,
            collectionId,
            createdAt.Ticks,
            documentId,
            queryFingerprint);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static QueryCursorPosition Decode(
        string token,
        Guid expectedProjectId,
        Guid expectedCollectionId,
        string expectedQueryFingerprint)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > QueryDocumentsCommand.MaximumPageTokenLength)
        {
            throw InvalidToken();
        }

        if (token.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_')))
        {
            throw InvalidToken();
        }

        try
        {
            var base64 = token.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');
            var bytes = Convert.FromBase64String(base64);

            if (bytes.Length is 0 or > MaximumDecodedTokenBytes)
            {
                throw InvalidToken();
            }

            var payload = JsonSerializer.Deserialize<CursorPayload>(bytes, JsonOptions);
            if (payload is null ||
                payload.Version != CurrentVersion ||
                payload.ProjectId != expectedProjectId ||
                payload.CollectionId != expectedCollectionId ||
                payload.DocumentId == Guid.Empty ||
                payload.CreatedAtTicks < DateTime.MinValue.Ticks ||
                payload.CreatedAtTicks > DateTime.MaxValue.Ticks ||
                !FixedTimeEquals(payload.QueryFingerprint, expectedQueryFingerprint))
            {
                throw InvalidToken();
            }

            return new QueryCursorPosition(
                new DateTime(payload.CreatedAtTicks, DateTimeKind.Utc),
                payload.DocumentId);
        }
        catch (FormatException)
        {
            throw InvalidToken();
        }
        catch (JsonException)
        {
            throw InvalidToken();
        }
    }

    private static bool FixedTimeEquals(string? actual, string expected)
    {
        if (string.IsNullOrEmpty(actual) || actual.Length != expected.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(actual),
            Encoding.UTF8.GetBytes(expected));
    }

    private static ArgumentException InvalidToken() =>
        new("Page token is invalid or does not match this project, collection, or query.", "PageToken");

    private sealed record CursorPayload(
        int Version,
        Guid ProjectId,
        Guid CollectionId,
        long CreatedAtTicks,
        Guid DocumentId,
        string QueryFingerprint);
}

internal readonly record struct QueryCursorPosition(
    DateTime CreatedAtUtc,
    Guid DocumentId);
