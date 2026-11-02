using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class HistorialReporteConfiguration : IEntityTypeConfiguration<HistorialReporte>
{
    public void Configure(EntityTypeBuilder<HistorialReporte> builder)
    {
        builder.ToTable("HistorialReportes");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.NombreArchivoExcel)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(h => h.IdUsuarioFiltro)
            .HasMaxLength(450);

        builder.Property(h => h.DestinatariosCorreo)
            .HasMaxLength(1000);

        builder.Property(h => h.DetalleErrorEnvio)
            .HasMaxLength(1000);

        builder.Property(h => h.CreatedBy).HasMaxLength(100);
        builder.Property(h => h.UpdatedBy).HasMaxLength(100);
        builder.Property(h => h.DeletedBy).HasMaxLength(100);

        builder.HasIndex(h => new { h.TipoReporte, h.Origen, h.FechaInicioReporte, h.EstadoEnvio })
            .HasDatabaseName("IX_HistorialReportes_ControlDiario");
    }
}
