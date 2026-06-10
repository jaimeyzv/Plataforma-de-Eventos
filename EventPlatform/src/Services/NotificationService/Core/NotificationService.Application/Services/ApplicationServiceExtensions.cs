using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.UseCases.ProcessEventCreated;

namespace NotificationService.Application.Services;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection ConfigureApplicationApp(this IServiceCollection services)
    {
        services.AddScoped<EventCreatedProcessor>();
        return services;
    }
}
