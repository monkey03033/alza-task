namespace Alza.Delivery.BackgroundJobs.Planning;

public sealed class VanTrip
{
    public VanTrip(string id, decimal maxWeight, decimal maxVolume)
    {
        Id = id;
        RemainingWeight = maxWeight;
        RemainingVolume = maxVolume;
    }

    public string Id { get; }
    public decimal RemainingWeight { get; private set; }
    public decimal RemainingVolume { get; private set; }
    public int PackageCount { get; private set; }
    public decimal Revenue { get; private set; }
    public bool IsFull => RemainingWeight <= 0 || RemainingVolume <= 0;

    public bool CanFit(decimal weight, decimal volume) =>
        weight <= RemainingWeight && volume <= RemainingVolume;

    public void Load(decimal weight, decimal volume, decimal revenue)
    {
        if (!CanFit(weight, volume))
            throw new InvalidOperationException($"Package does not fit into trip {Id}.");

        RemainingWeight -= weight;
        RemainingVolume -= volume;
        PackageCount++;
        Revenue += revenue;
    }

    public static IReadOnlyList<VanTrip> CreateFleet(VanConfig config)
    {
        var trips = new List<VanTrip>(config.VanCount * config.TripsPerVan);

        for (var van = 1; van <= config.VanCount; van++)
        {
            for (var trip = 0; trip < config.TripsPerVan; trip++)
            {
                var tripLabel = $"A{trip}";
                trips.Add(new VanTrip($"VAN-{van}-{tripLabel}", config.Weight, config.Volume));
            }
        }

        return trips;
    }
}
