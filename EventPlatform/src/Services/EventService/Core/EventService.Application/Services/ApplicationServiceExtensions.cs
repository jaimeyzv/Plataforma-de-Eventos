using System.Reflection;
using EventService.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Application.Services;

public static class ApplicationServiceExtensions
{
    /// <summary>Registers MediatR handlers, AutoMapper profiles, FluentValidation validators
    /// and the validation pipeline behavior for the application layer.</summary>
    public static IServiceCollection ConfigureApplicationApp(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
