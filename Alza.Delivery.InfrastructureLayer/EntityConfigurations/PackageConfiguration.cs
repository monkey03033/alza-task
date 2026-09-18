using Alza.Delivery.DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alza.Delivery.InfrastructureLayer.EntityConfigurations;

public sealed class PackageConfiguration
    : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Weight)
            .HasColumnType("numeric(10,2)");

        builder.Property(p => p.Volume)
            .HasColumnType("numeric(10,2)");

        builder.Property(p => p.Revenue)
            .HasColumnType("numeric(12,2)");
        
        builder.HasIndex(p => p.VanTripId)
            .HasFilter("\"VanTripId\" IS NULL")
            .IncludeProperties(p => new
            {
                p.Id,
                p.Weight,
                p.Volume,
                p.Revenue
            })
            .HasDatabaseName(
                "ix_packages_unassigned");
    }
}
