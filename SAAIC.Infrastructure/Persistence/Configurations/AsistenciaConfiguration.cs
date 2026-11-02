using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class AsistenciaConfiguration : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> builder)
    {
        builder.ToTable("Asistencias");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.IdUsuario)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.Observaciones)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedBy).HasMaxLength(100);
        builder.Property(a => a.UpdatedBy).HasMaxLength(100);
        builder.Property(a => a.DeletedBy).HasMaxLength(100);

        // Relación 1:1 con la instancia diaria de Reserva
        builder.HasOne(a => a.Reserva)
            .WithOne(r => r.Asistencia)
            .HasForeignKey<Asistencia>(a => a.IdReserva)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con ApplicationUser
        builder.HasOne(a => a.Usuario)
            .WithMany(u => u.Asistencias)
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        // Una sola hoja de asistencia por cada instancia diaria de reserva
        builder.HasIndex(a => a.IdReserva)
            .IsUnique()
            .HasDatabaseName("IX_Asistencias_IdReserva");

        // Índice para el corte diario automático y generación de reportes de asistencia
        builder.HasIndex(a => new { a.FechaEvento, a.Estado, a.Active })
            .HasDatabaseName("IX_Asistencias_FechaEvento_Estado");
    }
}
