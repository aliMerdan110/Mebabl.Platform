using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Infrastructure.Live;

public sealed class LiveStreamReconciliationService : BackgroundService
{
    private const string SrsApp = "live";
    private const int PageSize = 100;
    private const int MaxRecords = 100_000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LiveStreamReconciliationService> _logger;

    public LiveStreamReconciliationService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<LiveStreamReconciliationService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        SrsApiSettings settings;

        try
        {
            settings = LoadSettings();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "SRS reconciliation is disabled because its API settings are invalid.");

            return;
        }

        var intervalSeconds = Math.Clamp(
            _configuration.GetValue<int?>(
                "Srs:ReconciliationIntervalSeconds") ?? 15,
            10,
            300);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileAsync(settings, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SRS reconciliation failed. No status changes will be made from this failed check.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(intervalSeconds),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private SrsApiSettings LoadSettings()
    {
        var baseUrl = _configuration["Srs:ApiBaseUrl"];
        var username = _configuration["Srs:ApiUsername"];
        var password = _configuration["Srs:ApiPassword"];

        if (string.IsNullOrWhiteSpace(baseUrl) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Srs:ApiBaseUrl, Srs:ApiUsername and Srs:ApiPassword are required.");
        }

        if (!Uri.TryCreate(
                baseUrl,
                UriKind.Absolute,
                out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp &&
             baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Srs:ApiBaseUrl must be an absolute HTTP or HTTPS URL.");
        }

        if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
            baseUri = new Uri(baseUri.AbsoluteUri + "/");

        return new SrsApiSettings(baseUri, username, password);
    }

    private async Task ReconcileAsync(
        SrsApiSettings settings,
        CancellationToken cancellationToken)
    {
        // يجب أن ينجح الاستعلام الكامل قبل تغيير قاعدة البيانات.
        var publishedNames = await GetPublishedStreamNamesAsync(
            settings,
            cancellationToken);

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();

        var clock = scope.ServiceProvider
            .GetRequiredService<IClock>();

        var now = clock.UtcNow;

        // أسماء البث ليست فريدة حاليًا عبر جميع التطبيقات.
        // نتجاوز الأسماء الملتبسة كي لا نغيّر حالة تطبيق آخر.
        var owners = await db.LiveStreams
            .AsNoTracking()
            .Select(x => new StreamOwner(
                x.ApplicationId,
                x.Name))
            .ToListAsync(cancellationToken);

        var ambiguousNames = owners
            .GroupBy(x => x.Name, StringComparer.Ordinal)
            .Where(group =>
                group.Select(x => x.ApplicationId)
                    .Distinct()
                    .Skip(1)
                    .Any())
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        if (ambiguousNames.Count > 0)
        {
            _logger.LogDebug(
                "Skipping SRS reconciliation for {Count} stream names shared by multiple applications.",
                ambiguousNames.Count);
        }

        var sessions = await db.LiveStreamSessions
            .Include(x => x.LiveStream)
            .Where(x =>
                x.Status == LiveSessionStatus.Starting ||
                x.Status == LiveSessionStatus.Live)
            .ToListAsync(cancellationToken);

        var changed = false;

        foreach (var session in sessions)
        {
            var stream = session.LiveStream;

            if (stream is null ||
                string.IsNullOrWhiteSpace(stream.Name) ||
                ambiguousNames.Contains(stream.Name))
            {
                continue;
            }

            var isPublished = publishedNames.Contains(stream.Name);

            if (isPublished)
            {
                // إذا وصل النشر إلى SRS لكن لم تُحدّث الحالة،
                // يمكن استعادة الحالة من حالة SRS الفعلية.
                if (session.Status == LiveSessionStatus.Starting)
                {
                    session.Status = LiveSessionStatus.Live;
                    session.StartedAt ??= now;
                    changed = true;
                }

                if (stream.Status != LiveStreamStatus.Live)
                {
                    stream.Status = LiveStreamStatus.Live;
                    stream.UpdatedAt = now;
                    changed = true;
                }

                continue;
            }

            var startingTokenExpired =
                session.Status == LiveSessionStatus.Starting &&
                now >= session.PublishTokenExpiresAt;

            var liveStreamDisappeared =
                session.Status == LiveSessionStatus.Live;

            // لا ننهي Starting قبل انتهاء صلاحية الرمز.
            // ولا ننهي الجلسة إذا فشل الاستعلام؛ عندها لا نصل إلى هنا.
            if (!startingTokenExpired && !liveStreamDisappeared)
                continue;

            session.Status = LiveSessionStatus.Ended;
            session.EndedAt ??= now;
            changed = true;

            var hasOtherActiveSession = sessions.Any(other =>
                other.Id != session.Id &&
                other.LiveStreamId == session.LiveStreamId &&
                other.Status != LiveSessionStatus.Ended);

            if (!hasOtherActiveSession &&
                stream.Status != LiveStreamStatus.Offline)
            {
                stream.Status = LiveStreamStatus.Offline;
                stream.UpdatedAt = now;
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SRS reconciliation updated live session states.");
        }
    }

    private async Task<HashSet<string>> GetPublishedStreamNamesAsync(
        SrsApiSettings settings,
        CancellationToken cancellationToken)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var client = _httpClientFactory.CreateClient("SrsApi");

        for (var start = 0; start <= MaxRecords; start += PageSize)
        {
            var uri = new Uri(
                settings.BaseUri,
                $"api/v1/streams?start={start}&count={PageSize}");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                uri);

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{settings.Username}:{settings.Password}"));

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    credentials);

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var content =
                await response.Content.ReadAsStreamAsync(
                    cancellationToken);

            var page = await JsonSerializer.DeserializeAsync<SrsStreamsPage>(
                content,
                JsonOptions,
                cancellationToken);

            if (page is null ||
                page.Code != 0 ||
                page.Streams is null)
            {
                throw new InvalidOperationException(
                    "SRS returned an invalid streams response.");
            }

            foreach (var item in page.Streams)
            {
                if (string.IsNullOrWhiteSpace(item.App) ||
                    string.IsNullOrWhiteSpace(item.Name) ||
                    item.Publish?.Active is null)
                {
                    throw new InvalidOperationException(
                        "SRS returned an incomplete stream record.");
                }

                if (string.Equals(
                        item.App,
                        SrsApp,
                        StringComparison.Ordinal) &&
                    item.Publish.Active.Value)
                {
                    result.Add(item.Name);
                }
            }

            if (page.Streams.Count < PageSize)
                return result;
        }

        // لا نعتبر القائمة كاملة إذا تجاوزت الحد الذي نراجعه.
        throw new InvalidOperationException(
            "SRS stream pagination exceeded the reconciliation limit.");
    }

    private sealed record SrsApiSettings(
        Uri BaseUri,
        string Username,
        string Password);

    private sealed record StreamOwner(
        Guid ApplicationId,
        string Name);

    private sealed class SrsStreamsPage
    {
        [JsonPropertyName("code")]
        public int? Code { get; init; }

        [JsonPropertyName("streams")]
        public List<SrsStreamInfo>? Streams { get; init; }
    }

    private sealed class SrsStreamInfo
    {
        [JsonPropertyName("app")]
        public string? App { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("publish")]
        public SrsPublishInfo? Publish { get; init; }
    }

    private sealed class SrsPublishInfo
    {
        [JsonPropertyName("active")]
        public bool? Active { get; init; }
    }
}