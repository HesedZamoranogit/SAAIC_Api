using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SAAIC.Domain.Common;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Context;

/// <summary>
/// Contexto principal de persistencia para el Sistema de Reservas ITSON (EF Core + ASP.NET Core Identity).
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<TipoEspacio> TiposEspacio => Set<TipoEspacio>();
    public DbSet<Espacio> Espacios => Set<Espacio>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<ConfiguracionSistema> ConfiguracionesSistema => Set<ConfiguracionSistema>();
    public DbSet<HistorialReporte> HistorialReportes => Set<HistorialReporte>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de nombres de tablas de ASP.NET Identity sin prefijo 'AspNet'
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<IdentityRole>(b => b.ToTable("Roles"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("UserTokens"));

        // Aplica todas las clases IEntityTypeConfiguration<T> de SAAIC.Infrastructure
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                        entry.Entity.CreatedAt = ahora;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = ahora;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
