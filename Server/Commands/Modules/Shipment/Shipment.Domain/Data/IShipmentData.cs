namespace Shipment.Domain.Data;
using Core.Domain;
using Directories.Contracts;
using Shipment.Domain.Entities;

public interface IShipmentData : IData
{
    public Document.IRepository Shipments { get; }
    public Item.IRepository ShipmentItems { get; }
    public ClientContract.IClientProjectionRepository ClientProjections { get; }
    public MeasureUnitContract.IMeasureUnitProjectionRepository MeasureUnitProjections { get; }
    public ResourceContract.IResourceProjectionRepository ResourceProjections { get; }
}
