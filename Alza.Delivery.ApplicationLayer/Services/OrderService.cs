using Alza.Contracts;
using Alza.Delivery.ApplicationLayer.Abstractions;
using Alza.Delivery.DomainLayer.Models;
using Alza.Delivery.InfrastructureLayer;

namespace Alza.Delivery.ApplicationLayer.Services;

public class OrderService : IOrderService
{
    private readonly DeliveryDbContext _dbContext;

    public OrderService(DeliveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleOrder(CreateOrderRequest packages,
        CancellationToken cancellationToken)
    {
        var newPackages = packages.Packages.Select(p => new Package
        {
            Id = Guid.NewGuid(),
            Weight = p.Weight,
            Volume = p.Volume,
            Revenue = p.Revenue
        }).ToList();

        _dbContext.Packages.AddRange(newPackages);
        await _dbContext.SaveChangesAsync(cancellationToken);

    }
}