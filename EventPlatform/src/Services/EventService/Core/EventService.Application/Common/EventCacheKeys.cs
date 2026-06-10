namespace EventService.Application.Common;

public static class EventCacheKeys
{
    public const string AllEvents = "events:all";

    /// <summary>
    /// Holds the current "generation" of search results. Bumping it on every write
    /// invalidates all cached searches at once (their keys embed the generation).
    /// </summary>
    public const string SearchGeneration = "events:search:gen";

    public static string EventById(Guid id) => $"events:{id}";

    public static string Search(string generation, string token) =>
        $"events:search:{generation}:{token}";
}
