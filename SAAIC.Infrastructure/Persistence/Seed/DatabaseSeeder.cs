using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SAAIC.Domain.Common;
using SAAIC.Domain.Entities;
using SAAIC.Infrastructure.Persistence.Context;

namespace SAAIC.Infrastructure.Persistence.Seed;

/// <summary>
/// Semilla inicial de datos base del sistema ITSON (Roles, Tipos de Espacio base Aula/Foro, Campus y Configuración).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration? configuration = null)
    {
        // 1. Roles base del sistema: Profesor y Administrador
        string[] roles = { RolesSistema.Administrador, RolesSistema.Profesor };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Tipos de Espacio base: Aula y Foro
        if (!await context.TiposEspacio.AnyAsync())
        {
            var tiposBase = new List<TipoEspacio>
            {
                new()
                {
                    Nombre = "Aula",
                    Descripcion = "Espacio académico o cultural tipo aula para talleres, cursos y sesiones.",
                    CreatedBy = "SYSTEM"
                },
                new()
                {
                    Nombre = "Foro",
                    Descripcion = "Espacio cultural tipo foro o auditorio para conferencias, presentaciones y eventos.",
                    CreatedBy = "SYSTEM"
                }
            };

            await context.TiposEspacio.AddRangeAsync(tiposBase);
            await context.SaveChangesAsync();
        }

        // 3. Campus base de ITSON
        if (!await context.Campuses.AnyAsync())
        {
            var campusesBase = new List<Campus>
            {
                new() { Nombre = "Náinari", Codigo = "COB-NAI", Direccion = "Antonio Caso 2266, Villa ITSON, Cd. Obregón", CreatedBy = "SYSTEM" },
                new() { Nombre = "Centro", Codigo = "COB-CEN", Direccion = "5 de Febrero 818 Sur, Centro, Cd. Obregón", CreatedBy = "SYSTEM" },
                new() { Nombre = "Guaymas", Codigo = "GYM", Direccion = "Carretera Internacional Km 4, Guaymas", CreatedBy = "SYSTEM" },
                new() { Nombre = "Navojoa", Codigo = "NAV-CENTRO", Direccion = "Ramon Corona y S. Otero, Navojoa", CreatedBy = "SYSTEM" },
                new() { Nombre = "Empalme", Codigo = "EMP", Direccion = "Empalme, Sonora", CreatedBy = "SYSTEM" }
            };

            await context.Campuses.AddRangeAsync(campusesBase);
            await context.SaveChangesAsync();
        }

        // 4. Configuración global del sistema (tiempo de tolerancia de retardo y reportes automáticos)
        if (!await context.ConfiguracionesSistema.AnyAsync())
        {
            var adminReportEmail = configuration?["SeedAdmin:Email"] ?? "reportes@itson.edu.mx";

            var configInicial = new ConfiguracionSistema
            {
                MinutosToleranciaRetardo = 15,
                MinutosAnticipacionCheckIn = 30,
                EnvioAutomaticoReportesActivo = true,
                HoraEnvioReporteEventos = new TimeOnly(7, 0),
                HoraEnvioReporteAsistencia = new TimeOnly(22, 0),
                CorreosDestinatariosReportes = adminReportEmail,
                CreatedBy = "SYSTEM"
            };

            await context.ConfiguracionesSistema.AddAsync(configInicial);
            await context.SaveChangesAsync();
        }

        // 5. Usuario Administrador inicial por defecto (solo si se configura explícitamente en User Secrets o configuración)
        var defaultAdminPassword = configuration?["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(defaultAdminPassword))
        {
            var defaultAdminId = configuration?["SeedAdmin:IdInstitucional"] ?? "112233";
            var defaultAdminEmail = configuration?["SeedAdmin:Email"] ?? "admin@itson.edu.mx";
            var defaultAdminNombre = configuration?["SeedAdmin:NombreCompleto"] ?? "Administrador General ITSON";

            var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.IdInstitucional == defaultAdminId)
                ?? await userManager.FindByEmailAsync(defaultAdminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = defaultAdminId,
                    Email = defaultAdminEmail,
                    EmailConfirmed = true,
                    NombreCompleto = defaultAdminNombre,
                    IdInstitucional = defaultAdminId,
                    Departamento = "Extensión de la Cultura",
                    Active = true
                };

                var result = await userManager.CreateAsync(adminUser, defaultAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, RolesSistema.Administrador);
                }
            }
        }
    }
}
