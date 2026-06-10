using System.Reflection;
using EventService.Application.Abstractions;
using EventService.Application.Repositories;
using EventService.Infrastructure.Caching;
using EventService.Infrastructure.Messaging;
using EventService.Infrastructure.Persistence.Context;
using EventService.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure.Services;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection ConfigureInfrastructureApp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        AddPersistence(services, configuration);
        AddCache(services, configuration);
        AddMessaging(services, configuration);

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<EventDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                // Resilience: automatic retry on transient SQL Server faults (deadlocks, timeouts).
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sql.MigrationsAssembly(typeof(EventDbContext).Assembly.FullName);
            }));
    }

    private static void AddCache(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnection))
        {
            // Fallback so the API runs even without Redis (e.g. unit/dev runs).
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "eventservice:";
            });
        }

        services.AddScoped<ICacheService, RedisCacheService>();
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        var rabbit = configuration.GetSection("RabbitMq");
        var host = rabbit["Host"] ?? "localhost";
        var virtualHost = rabbit["VirtualHost"] ?? "/";
        var username = rabbit["Username"] ?? "guest";
        var password = rabbit["Password"] ?? "guest";
        var port = ushort.TryParse(rabbit["Port"], out var p) ? p : (ushort)5672;

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // Transactional outbox bound to the EventDbContext.
            x.AddEntityFrameworkOutbox<EventDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(1);
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Resilience: exponential retry for transient broker/consumer faults.
                cfg.UseMessageRetry(r =>
                    r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
