using Microsoft.EntityFrameworkCore;
using Tablewise.Infrastructure;
using Tablewise.Infrastructure.Persistence;
using Tablewise.Infrastructure.Persistence.SeedData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Infrastructure (DbContext, Repositories, Context Services)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Development ortamında seed data çalıştır
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    
    try
    {
        var context = services.GetRequiredService<TablewiseDbContext>();
        var logger = services.GetRequiredService<ILogger<DbSeeder>>();
        
        // Migration'ları uygula
        await context.Database.MigrateAsync();
        
        // Seed data ekle
        var seeder = new DbSeeder(context, logger);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed data işlemi sırasında hata oluştu.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
