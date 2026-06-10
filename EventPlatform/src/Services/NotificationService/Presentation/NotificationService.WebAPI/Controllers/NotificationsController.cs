using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;

namespace NotificationService.WebAPI.Controllers;

/// <summary>Read-only view over the notification audit log (handy to verify the async flow).</summary>
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationLogRepository _repository;

    public NotificationsController(INotificationLogRepository repository) => _repository = repository;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var logs = await _repository.GetRecentAsync(Math.Clamp(limit, 1, 200), cancellationToken);
        return Ok(logs);
    }
}
