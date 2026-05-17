namespace Balance.Domain.Data;
using Shipment.Contracts;

public interface IBalanceData
{
    public Entities.Balance.IRepository Balances { get; }
    public ShipmentItemContract.IShipmentItemProjectionRepository ShipmentItemProjections { get; }
}
