using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SAAIC.Application;
using SAAIC.Domain.Entities;
using SAAIC.Infrastructure;
using SAAIC.Infrastructure.Persistence.Context;
using SAAIC.Infrastructure.Persistence.Seed;
using SAAIC.Web.Extensions;
using SAAIC.Web.Middlewares;
using SAAIC.Web.Services;


var builder = WebApplication.CreateBuilder(args);

// 1. Servicios de las capas de la arquitectura (Application e Infrastructure)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Autenticación JWT Bearer y Autorización por Roles
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtAuthenticationHandler.SchemeName;
        options.DefaultChallengeScheme = JwtAuthenticationHandler.SchemeName;
    })
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(
        JwtAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization();

// 3. Servicio en segundo plano para inasistencias y envío automático diario de reportes Excel
builder.Services.AddHostedService<ReportesDiariosBackgroundService>();

// 4. Configuración de CORS para el cliente React en la Intranet
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowIntranetClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 5. Configuración de ASP.NET Core Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SAAIC API - Sistema de Reservas ITSON",
        Description = "Documentación interactiva de endpoints y APIs del sistema de reservas de espacios culturales (Aulas y Foros)"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var usermanager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        // Aplica automáticamente las migraciones pendientes al correr el proyecto
        await context.Database.MigrateAsync();

        // Ejecuto la carga inicial
        await DatabaseSeeder.SeedAsync(context, roleManager, usermanager, configuration);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fallo critico al aplicar migraciones o sembrar datos en el arranque.");
    }
}

// Middleware global de excepciones
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API v1");

        // Carga Swagger UI directamente en la raiz de la aplicacion
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowIntranetClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
