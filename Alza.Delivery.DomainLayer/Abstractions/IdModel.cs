namespace Alza.Delivery.DomainLayer.Abstractions;

public abstract class IdModel<TKey> where TKey : notnull
{
    public virtual TKey Id { get; set; } = default!;
}