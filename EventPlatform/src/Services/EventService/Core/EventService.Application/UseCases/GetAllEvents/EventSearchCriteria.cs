using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EventService.Application.UseCases.GetAllEvents;

/// <summary>
/// Optional filters for the advanced event search. All provided criteria are AND-combined
/// and applied at the database level by the repository.
/// </summary>
public sealed record EventSearchCriteria
{
    /// <summary>Free text matched against the event name or place.</summary>
    public string? Text { get; init; }

    /// <summary>Lower bound for the event date (inclusive).</summary>
    public DateTimeOffset? From { get; init; }

    /// <summary>Upper bound for the event date (inclusive).</summary>
    public DateTimeOffset? To { get; init; }

    /// <summary>Only events with at least one zone priced at or below this value.</summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>Exact event status, e.g. "Published".</summary>
    public string? Status { get; init; }

    /// <summary>True when no filter is set (equivalent to "list everything").</summary>
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Text)
        && From is null
        && To is null
        && MaxPrice is null
        && string.IsNullOrWhiteSpace(Status);

    /// <summary>
    /// Stable, normalized token that uniquely identifies this set of criteria. Used to build
    /// the Redis cache key so identical searches share a cached response.
    /// </summary>
    public string CacheToken()
    {
        var raw = string.Join(
            '|',
            Text?.Trim().ToLowerInvariant() ?? string.Empty,
            From?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            To?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            MaxPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            Status?.Trim().ToLowerInvariant() ?? string.Empty);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}
