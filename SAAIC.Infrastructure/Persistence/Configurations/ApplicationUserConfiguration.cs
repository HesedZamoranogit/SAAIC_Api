using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAAIC.Domain.Entities;

namespace SAAIC.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.NombreCompleto)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.IdInstitucional)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.Departamento)
            .HasMaxLength(120);

        builder.HasIndex(u => u.IdInstitucional)
            .IsUnique()
            .HasDatabaseName("IX_Users_IdInstitucional");
    }
}
