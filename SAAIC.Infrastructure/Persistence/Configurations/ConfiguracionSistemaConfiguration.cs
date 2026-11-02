using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class ConfiguracionSistemaConfiguration : IEntityTypeConfiguration<ConfiguracionSistema>
{
    public void Configure(EntityTypeBuilder<ConfiguracionSistema> builder)
    {
        builder.ToTable("ConfiguracionesSistema");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CorreosDestinatariosReportes)
            .HasMaxLength(1000);

        builder.Property(c => c.CreatedBy).HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);
        builder.Property(c => c.DeletedBy).HasMaxLength(100);
    }
}
