using Alza.Delivery.DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace Alza.Delivery.InfrastructureLayer;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
    }

    public DbSet<Package> Packages => Set<Package>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
