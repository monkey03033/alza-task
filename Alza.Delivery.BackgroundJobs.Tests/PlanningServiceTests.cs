using Alza.Delivery.BackgroundJobs.Planning;
using Alza.Delivery.BackgroundJobs.Tests.Fixtures;
using Alza.Delivery.DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Alza.Delivery.BackgroundJobs.Tests;

public class PlanningServiceTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task PlanDeliveries_FillsVansWithHighestRevenuePackages_AndPersistsAssignments()
    {
        // Arrange
        var vanConfig = new VanConfig { VanCount = 2, TripsPerVan = 2, Weight = 10m, Volume = 10m };

        await using var seedContext = fixture.CreateDbContext();
        
        var packages = Enumerable.Range(1, 5)
            .Select(i => new Package { Weight = 10m, Volume = 10m, Revenue = i * 10m })
            .ToList();
        seedContext.Packages.AddRange(packages);
        await seedContext.SaveChangesAsync();

        await using var planningContext = fixture.CreateDbContext();
        var planningService = new PlanningService(planningContext, Options.Create(vanConfig));

        // Act
        var result = await planningService.PlanDeliveries(CancellationToken.None);

        // Assert
        Assert.Equal(4, result.Assignments.Count);
        Assert.Equal(4, result.Trips.Count(t => t.PackageCount == 1));
        Assert.Equal(140m, result.TotalRevenue);

        var cheapestPackageId = packages.OrderBy(p => p.Revenue).First().Id;
        Assert.DoesNotContain(result.Assignments, a => a.PackageId == cheapestPackageId);

        await using var verifyContext = fixture.CreateDbContext();
        var assignedCount = await verifyContext.Packages.CountAsync(p => p.VanTripId != null);
        var cheapestStillUnassigned = await verifyContext.Packages
            .Where(p => p.Id == cheapestPackageId)
            .Select(p => p.VanTripId)
            .SingleAsync();

        Assert.Equal(4, assignedCount);
        Assert.Null(cheapestStillUnassigned);
    }
}