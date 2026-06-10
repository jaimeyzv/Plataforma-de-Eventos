using EventService.Application.Services;
using EventService.Infrastructure.Services;
using EventService.WebAPI.Extensions;
using EventService.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Composition root: each layer registers its own services.
builder.Services.ConfigureApplicationApp();
builder.Services.ConfigureInfrastructureApp(builder.Configuration);
builder.Services.ConfigureCorsPolicy(builder.Configuration);
builder.Services.ConfigureJwtAuth(builder.Configuration);
builder.Services.ConfigureSwagger();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(CorsPolicyExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
