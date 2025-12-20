using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Enhanced Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT Authentication (Optional - আপাতত disable রাখুন login এর জন্য)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[GATEWAY] Auth failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"[GATEWAY] Token validated for: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add CORS - VERY IMPORTANT for Blazor
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware Order is CRITICAL
app.UseCors();

// Request Logging Middleware
app.Use(async (context, next) =>
{
    var requestTime = DateTime.UtcNow;
    Console.WriteLine($"[GATEWAY] ===== Incoming Request =====");
    Console.WriteLine($"[GATEWAY] Time: {requestTime:yyyy-MM-dd HH:mm:ss.fff}");
    Console.WriteLine($"[GATEWAY] Method: {context.Request.Method}");
    Console.WriteLine($"[GATEWAY] Path: {context.Request.Path}");
    Console.WriteLine($"[GATEWAY] Query: {context.Request.QueryString}");
    Console.WriteLine($"[GATEWAY] Origin: {context.Request.Headers["Origin"]}");
    Console.WriteLine($"[GATEWAY] Content-Type: {context.Request.ContentType}");

    await next();

    var responseTime = DateTime.UtcNow;
    var duration = (responseTime - requestTime).TotalMilliseconds;
    Console.WriteLine($"[GATEWAY] ===== Response =====");
    Console.WriteLine($"[GATEWAY] Status: {context.Response.StatusCode}");
    Console.WriteLine($"[GATEWAY] Duration: {duration:F2}ms");
    Console.WriteLine($"[GATEWAY] ====================");
});

// Authentication & Authorization (Login endpoint এ apply হবে না)
app.UseAuthentication();
app.UseAuthorization();

// Map YARP reverse proxy
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        Console.WriteLine($"[GATEWAY PROXY] Forwarding to: {context.Request.Path}");
        await next();
        Console.WriteLine($"[GATEWAY PROXY] Response from backend: {context.Response.StatusCode}");
    });
});

Console.WriteLine("[GATEWAY] =================================");
Console.WriteLine("[GATEWAY] API Gateway Started Successfully");
Console.WriteLine($"[GATEWAY] Listening on: {builder.Configuration["urls"] ?? "https://localhost:44308"}");
Console.WriteLine("[GATEWAY] =================================");

app.Run();
