using Microsoft.OpenApi.Models;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureApplicationApp();
builder.Services.ConfigureInfrastructureApp(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NotificationService API",
        Version = "v1",
        Description = "Event Platform – Notification microservice (RabbitMQ consumer, idempotent, MongoDB)."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
