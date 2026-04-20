using Tablewise.Infrastructure;
using Tablewise.Infrastructure.Services;
using Tablewise.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Infrastructure (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Geçici: Design-time services (gerçek implementation sonraki fazda)
builder.Services.AddScoped<ITenantContext, DesignTimeTenantContext>();
builder.Services.AddScoped<ICurrentUser, DesignTimeCurrentUser>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
