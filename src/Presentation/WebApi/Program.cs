using Application;
using Identity;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Persistence;
using Persistence.Contexts;
using Serilog;
using Shared;
using System.Text.Json.Serialization;
using WebApi.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//Application Layer
builder.Services.AddApplicationLayer(builder.Configuration);

//Identity Layer
builder.Services.AddIdentityInfrastructureLayer(builder.Configuration);

//Persistence Layer
builder.Services.AddPersistenceLayer(builder.Configuration);
//Shared Layer
builder.Services.AddSharedLayer(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

//Agrego instancia para versionado
builder.Services.AddApiVersioningExtension();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
       builder =>
       {
           builder.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
       });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//Aca usamos el middleware de errores
//app.UseErrorHandlingMiddleware();

app.MapControllers();

try
{
    Log.Information("Iniciando Web API");

    await CargarSeeds();

    Log.Information("Corriendo en:");
    Log.Information("https://localhost:44361");

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");

    return;
}
finally
{
    Log.CloseAndFlush();
}

app.Run();

async Task CargarSeeds()
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();


    //await DefaultRoles.SeedAsync(userManager, roleManager);
    //await DefaultUser.SeedAsync(userManager, roleManager, context);

}