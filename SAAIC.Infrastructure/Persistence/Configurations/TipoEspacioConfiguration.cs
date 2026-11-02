using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class TipoEspacioConfiguration : IEntityTypeConfiguration<TipoEspacio>
{
    public void Configure(EntityTypeBuilder<TipoEspacio> builder)
    {
        builder.ToTable("TiposEspacio");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(t => t.Descripcion)
            .HasMaxLength(200);

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100);

        builder.HasIndex(t => t.Nombre)
            .IsUnique()
            .HasDatabaseName("IX_TiposEspacio_Nombre");
    }
}
