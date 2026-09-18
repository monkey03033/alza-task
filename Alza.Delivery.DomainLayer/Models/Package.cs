using Alza.Delivery.DomainLayer.Abstractions;

namespace Alza.Delivery.DomainLayer.Models;

public class Package : IdModel<Guid>, IEntity
{
    public decimal Weight { get; set; }

    public decimal Volume { get; set; }

    public decimal Revenue { get; set; }

    public string? VanTripId { get; private set; }
}