namespace Alza.Delivery.BackgroundJobs.Planning;

public sealed class VanConfig
{
    public int VanCount { get; set; }
    public int TripsPerVan { get; set; }
    public decimal Weight { get; set; }
    public decimal Volume { get; set; }
}