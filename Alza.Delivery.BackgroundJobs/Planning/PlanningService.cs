using Alza.Delivery.InfrastructureLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Alza.Delivery.BackgroundJobs.Planning;

public sealed class PlanningService : IPlanningService
{
    private readonly DeliveryDbContext _dbContext;
    private readonly VanConfig _vanConfig;

    private const int PageSize = 5_000;
    private (decimal Score, Guid Id)? _continuation;

    public PlanningService(DeliveryDbContext dbContext, IOptions<VanConfig> options)
    {
        _dbContext = dbContext;
        _vanConfig = options.Value;
    }

    public async Task<PlanningResult> PlanDeliveries(CancellationToken cancellationToken)
    {
        var trips = VanTrip.CreateFleet(_vanConfig);
        var openTrips = new List<VanTrip>(trips);
        var assignments = new List<PackageAssignment>();

        while (openTrips.Count > 0)
        {
            var maxWeight = openTrips.Max(t => t.RemainingWeight);
            var maxVolume = openTrips.Max(t => t.RemainingVolume);

            var packages = await FetchNextCandidates(maxWeight, maxVolume, cancellationToken);
            if (packages.Count == 0)
                break;
            
            foreach (var package in packages)
            {
                if (openTrips.Count == 0)
                    break;

                var trip = openTrips.FirstOrDefault(t => t.CanFit(package.Weight, package.Volume));
                if (trip is null)
                    continue;

                trip.Load(package.Weight, package.Volume, package.Revenue);
                assignments.Add(new PackageAssignment(package.Id, trip.Id));

                if (trip.IsFull)
                    openTrips.Remove(trip);
            }
        }

        var tripResults = trips
            .Select(trip => new VanTripPlanningResult(
                trip.Id,
                trip.PackageCount,
                trip.RemainingWeight,
                trip.RemainingVolume,
                trip.Revenue))
            .ToArray();

        var result = new PlanningResult(assignments, tripResults, trips.Sum(t => t.Revenue));

        if (result.Assignments.Count > 0)
            await SaveAssignments(result.Assignments, cancellationToken);

        return result;
    }

    private async Task SaveAssignments(IReadOnlyList<PackageAssignment> assignments, CancellationToken cancellationToken)
    {
        var ids = assignments.Select(a => a.PackageId).ToArray();
        var tripIds = assignments.Select(a => a.VanTripId).ToArray();

        var idsParam = new NpgsqlParameter("ids", ids);
        var tripIdsParam = new NpgsqlParameter("tripIds", tripIds);

        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            UPDATE "Packages" AS p
            SET "VanTripId" = a."VanTripId"
            FROM (SELECT UNNEST(@ids) AS "Id", UNNEST(@tripIds) AS "VanTripId") AS a
            WHERE p."Id" = a."Id"
            """, [idsParam, tripIdsParam], cancellationToken);
    }

    private async Task<IReadOnlyList<PackageCandidate>> FetchNextCandidates(
        decimal maxWeight, decimal maxVolume, CancellationToken cancellationToken)
    {
        var query = _dbContext.Packages
            .Where(p => p.VanTripId == null && p.Revenue > 0 && p.Weight <= maxWeight && p.Volume <= maxVolume)
            .Select(p => new
            {
                p.Id,
                p.Weight,
                p.Volume,
                p.Revenue,
                Score = p.Revenue / (p.Weight / _vanConfig.Weight + p.Volume / _vanConfig.Volume)
            });

        if (_continuation is { } after)
            query = query.Where(p =>
                p.Score < after.Score ||
                (p.Score == after.Score && p.Id.CompareTo(after.Id) > 0));

        var page = await query
            .OrderByDescending(p => p.Score)
            .ThenBy(p => p.Id)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        if (page.Count > 0)
            _continuation = (page[^1].Score, page[^1].Id);

        return page.Select(p => new PackageCandidate(p.Id, p.Weight, p.Volume, p.Revenue)).ToList();
    }
}