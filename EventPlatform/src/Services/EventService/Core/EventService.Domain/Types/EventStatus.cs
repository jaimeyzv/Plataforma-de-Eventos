namespace EventService.Domain.Types;

/// <summary>
/// Lifecycle of an event. Persisted as a short string in SQL for readability.
/// </summary>
public enum EventStatus
{
    Draft = 0,
    Published = 1,
    Cancelled = 2
}
