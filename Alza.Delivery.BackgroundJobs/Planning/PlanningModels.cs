namespace Alza.Delivery.BackgroundJobs.Planning;

public sealed record PackageCandidate(Guid Id, decimal Weight, decimal Volume, decimal Revenue);

public sealed record PackageAssignment(Guid PackageId, string VanTripId);

public sealed record VanTripPlanningResult(
    string VanId,
    int PackageCount,
    decimal RemainingWeight,
    decimal RemainingVolume,
    decimal Revenue);

public sealed record PlanningResult(
    IReadOnlyList<PackageAssignment> Assignments,
    IReadOnlyList<VanTripPlanningResult> Trips,
    decimal TotalRevenue);