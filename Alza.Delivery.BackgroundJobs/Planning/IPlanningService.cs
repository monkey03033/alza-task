namespace Alza.Delivery.BackgroundJobs.Planning;

public interface IPlanningService
{
    Task<PlanningResult> PlanDeliveries(CancellationToken cancellationToken);
}