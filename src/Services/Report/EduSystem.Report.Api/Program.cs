var builder = WebApplication.CreateBuilder(args);
// Health checks
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
