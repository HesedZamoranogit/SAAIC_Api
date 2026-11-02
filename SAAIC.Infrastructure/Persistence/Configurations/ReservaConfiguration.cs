using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("Reservas");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.IdUsuario)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.ResponsableEvento)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Descripcion)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.NotaSeguridad)
            .HasMaxLength(500);

        builder.Property(r => r.MotivoCancelacion)
            .HasMaxLength(350);

        builder.Property(r => r.CanceladaPorUsuarioId)
            .HasMaxLength(450);

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.UpdatedBy).HasMaxLength(100);
        builder.Property(r => r.DeletedBy).HasMaxLength(100);

        // Relación con Espacio
        builder.HasOne(r => r.Espacio)
            .WithMany(e => e.Reservas)
            .HasForeignKey(r => r.IdEspacio)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con Campus
        builder.HasOne(r => r.Campus)
            .WithMany(c => c.Reservas)
            .HasForeignKey(r => r.IdCampus)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación con ApplicationUser (Profesor/Admin dueño de la reserva)
        builder.HasOne(r => r.Usuario)
            .WithMany(u => u.Reservas)
            .HasForeignKey(r => r.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Auto-Referenciada Padre-Hija (Jornada multi-día y Semestral)
        builder.HasOne(r => r.ReservaPadre)
            .WithMany(r => r.ReservasHijas)
            .HasForeignKey(r => r.IdReservaPadre)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice compuesto crítico para verificación instantánea de disponibilidad y prevención de choques de horario
        builder.HasIndex(r => new
        {
            r.IdEspacio,
            r.FechaInicio,
            r.EsReservaPadre,
            r.Estado,
            r.HoraInicio,
            r.HoraFin
        })
        .HasDatabaseName("IX_Reservas_Disponibilidad_Concurrencia");

        // Índice para consultas rápidas de reservas por usuario
        builder.HasIndex(r => new { r.IdUsuario, r.FechaInicio })
            .HasDatabaseName("IX_Reservas_Usuario_Fecha");
    }
}
