using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Domain.Entities;
using SAAIC.Infrastructure.Persistence.Context;
using SAAIC.Infrastructure.Persistence.Repositories;
using SAAIC.Infrastructure.Services;


namespace SAAIC.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra DbContext (SQL Server), ASP.NET Core Identity, Repositorios, UnitOfWork y Servicios de Infraestructura (JWT, Email, Excel).
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Base de Datos SQL Server
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));



        // 3. Repositorios y Unidad de Trabajo (Transacciones y Bloqueo Pesimista)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEspacioRepository, EspacioRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
        services.AddScoped<IConfiguracionSistemaRepository, ConfiguracionSistemaRepository>();
        services.AddScoped<IHistorialReporteRepository, HistorialReporteRepository>();

        // 4. Opciones de configuración (JWT y Correo)
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        // 5. Servicios Técnicos Externos
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IExcelReportGenerator, ExcelReportGenerator>();

        return services;
    }
}
