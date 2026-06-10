using EventService.Application.Common;
using EventService.Application.UseCases.CreateEvent;
using EventService.Application.UseCases.GetAllEvents;
using EventService.Application.UseCases.GetEventById;
using EventService.Domain.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.WebAPI.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates an event with its zones (Admin only) and publishes EventCreated.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CreateEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateEventResponse>> Create(
        [FromBody] CreateEventRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Event.EventId }, response);
    }

    /// <summary>
    /// Advanced search over events (Redis-cached). Public read. All filters are optional and
    /// AND-combined. This being a public endpoint, it defaults to <c>Published</c> events;
    /// pass an explicit <c>status</c> (e.g. Draft, Cancelled) to override.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetAllEventsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAllEventsResponse>> GetAll(
        [FromQuery] string? text,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? status,
        CancellationToken cancellationToken = default)
    {
        var request = new GetAllEventsRequest
        {
            Criteria = new EventSearchCriteria
            {
                Text = text,
                From = from,
                To = to,
                MaxPrice = maxPrice,
                Status = status ?? nameof(EventStatus.Published),
            }
        };

        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Gets a single event by id (Redis-cached).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetEventByIdRequest(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
