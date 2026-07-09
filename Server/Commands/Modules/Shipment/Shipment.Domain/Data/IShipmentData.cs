namespace Shipment.Domain.Data;

using Core.Domain;
using Shipment.Domain.Entities;

public interface IShipmentData : IData
{
    public Document.IRepository Shipments { get; }
    public Item.IRepository ShipmentItems { get; }
}
