namespace Shipment.Contracts;
using Core.Domain;
using Shipment.Domain.Data;


public static class Lookup
{
    public record ShipmentItemDTO(Guid ShipmentGuid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);

    public async static Task<List<ShipmentItemDTO>> GetShipmentItemsByShipmentGuidsAsync(HashSet<Guid> guids, IData data)
    {
        var sData = (IShipmentData)data;
        await sData.ShipmentItems.EnsureByShipmentGuids(guids);

        return sData.ShipmentItems.List
            .Where(x => guids.Contains(x.ShipmentGuid))
            .Select(x => new ShipmentItemDTO(x.ShipmentGuid, x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();
    }
}
