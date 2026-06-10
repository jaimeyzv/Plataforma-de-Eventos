using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NotificationService.Application.Abstractions;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Notifications;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;

namespace NotificationService.Infrastructure.Services;

public static class InfrastructureServiceExtensions
{
    private static bool _serializersRegistered;

    public static IServiceCollection ConfigureInfrastructureApp(this IServiceCollection services, IConfiguration configuration)
    {
        AddMongo(services, configuration);
        AddNotifications(services, configuration);
        AddMessaging(services, configuration);
        return services;
    }

    private static void AddMongo(IServiceCollection services, IConfiguration configuration)
    {
        RegisterMongoSerializers();

        services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration.GetSection(MongoSettings.SectionName)["ConnectionString"]
                                   ?? "mongodb://localhost:27017";
            return new MongoClient(connectionString);
        });

        services.AddSingleton<MongoContext>();
        services.AddScoped<IProcessedMessageStore, MongoProcessedMessageStore>();
        services.AddScoped<INotificationLogRepository, MongoNotificationLogRepository>();
    }

    private static void AddNotifications(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<INotificationSender, SmtpNotificationSender>();
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
            x.AddConsumer<EventCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, port, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Resilience: exponential retry, then dead-letter on repeated failure.
                cfg.UseMessageRetry(r =>
                    r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    private static void RegisterMongoSerializers()
    {
        if (_serializersRegistered) return;
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        _serializersRegistered = true;
    }
}
