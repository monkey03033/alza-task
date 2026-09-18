using Alza.Contracts;

namespace Alza.Delivery.ApplicationLayer.Abstractions;

public interface IOrderService
{
    Task HandleOrder(CreateOrderRequest packages,
        CancellationToken cancellationToken);
}