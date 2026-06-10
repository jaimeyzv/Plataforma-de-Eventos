namespace EventService.WebAPI.Extensions;

public static class CorsPolicyExtensions
{
    public const string PolicyName = "DevCors";

    public static IServiceCollection ConfigureCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        return services;
    }
}
