using EduSystem.Identity.Api.Endpoints;
using EduSystem.Identity.Application.Commands;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using EduSystem.Identity.Infrastructure.Repositories;
using EduSystem.Identity.Infrastructure.Service;
using EduSystem.Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using EduSystem.Shared.Messaging.Extensions;
using EduSystem.Shared.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDBConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterTenantCommand).Assembly);
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITenantDatabaseProvisioner, TenantDatabaseProvisioner>();
builder.Services.AddSingleton<IConnectionStringEncryptor, ConnectionStringEncryptor>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEventBus(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapTenantEndpoints();

app.Run();
