namespace Shipment.Contracts;

public static class ShipmentItemContract
{
    public interface IShipmentItemProjection
    {
        Guid ShipmentGuid { get; }
        Guid ResourceGuid { get; }
        Guid MeasureUnitGuid { get; }
        decimal Quantity { get; }
    }

    public interface IShipmentItemProjectionRepository
    {
        IEnumerable<IShipmentItemProjection> List { get; }
        Task EnsureByShipmentGuids(HashSet<Guid> guids);
    }
}
