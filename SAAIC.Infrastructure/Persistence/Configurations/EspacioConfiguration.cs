using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class EspacioConfiguration : IEntityTypeConfiguration<Espacio>
{
    public void Configure(EntityTypeBuilder<Espacio> builder)
    {
        builder.ToTable("Espacios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(e => e.HoraApertura)
            .IsRequired();

        builder.Property(e => e.HoraCierre)
            .IsRequired();

        builder.Property(e => e.Estado)
            .IsRequired();

        builder.Property(e => e.CapacidadMaxima)
            .IsRequired();

        // Token de concurrencia optimista nativo en SQL Server
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.DeletedBy).HasMaxLength(100);

        builder.HasOne(e => e.Campus)
            .WithMany(c => c.Espacios)
            .HasForeignKey(e => e.IdCampus)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TipoEspacio)
            .WithMany(t => t.Espacios)
            .HasForeignKey(e => e.IdTipoEspacio)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.IdCampus, e.Nombre })
            .IsUnique()
            .HasDatabaseName("IX_Espacios_Campus_Nombre");
    }
}
