using EduSystem.Attendance.Infrastructure.EventHandlers;
using EduSystem.Attendance.Infrastructure.Contexts;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
// Connection String Encryptor
builder.Services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();
builder.Services.AddDbContext<AttendanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDBConnection")));

// MassTransit with Consumer
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TenantDatabaseCreatedEventHandler>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqPort = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672);
        var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitMqHost, (ushort)rabbitMqPort, "/", h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromMinutes(5),
            intervalDelta: TimeSpan.FromSeconds(2)
        ));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IEventBus, MassTransitEventBus>();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");
app.Run();