using FluentValidation;

namespace EventService.Application.UseCases.CreateEvent;

/// <summary>
/// Server-side validation mirroring the rules required by the challenge:
/// required fields, capacity &gt; 0, price &gt;= 0. Runs in a MediatR pipeline behavior.
/// </summary>
public sealed class CreateEventValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Place)
            .NotEmpty().WithMessage("Event place is required.")
            .MaximumLength(200);

        RuleFor(x => x.EventDate)
            .NotEmpty().WithMessage("Event date is required.");

        RuleFor(x => x.Zones)
            .NotEmpty().WithMessage("At least one zone is required.");

        RuleForEach(x => x.Zones).ChildRules(zone =>
        {
            zone.RuleFor(z => z.Name)
                .NotEmpty().WithMessage("Zone name is required.")
                .MaximumLength(100);

            zone.RuleFor(z => z.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Zone price must be greater than or equal to 0.");

            zone.RuleFor(z => z.Capacity)
                .GreaterThan(0).WithMessage("Zone capacity must be greater than 0.");
        });
    }
}
