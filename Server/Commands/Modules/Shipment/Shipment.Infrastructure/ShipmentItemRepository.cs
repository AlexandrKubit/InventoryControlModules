namespace Shipment.Infrastructure;

using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shipment.Domain.Entities;
using static Shipment.Contracts.ShipmentItemContract;

public class ShipmentItemRepository : BaseRepository<Item>, Item.IRepository, IShipmentItemProjectionRepository
{

    public class ShipmentItemProjection : IShipmentItemProjection
    {
        public Guid ShipmentGuid { get; }
        public Guid ResourceGuid { get; }
        public Guid MeasureUnitGuid { get; }
        public decimal Quantity { get; }

        public ShipmentItemProjection(Item item)
        {
            ShipmentGuid = item.ShipmentGuid;
            ResourceGuid = item.ResourceGuid;
            MeasureUnitGuid = item.MeasureUnitGuid;
            Quantity = item.Quantity;
        }
    }

    IEnumerable<IShipmentItemProjection> IShipmentItemProjectionRepository.List => List.Select(x => new ShipmentItemProjection(x));

    private Item Restore(Core.Infrastructure.Entities.ShipmentItem item) =>
        Item.IRepository.Restore(item.Guid, item.ShipmentGuid, item.ResourceGuid, item.MeasureUnitGuid, item.Quantity);

    public async Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids)
    {
        var func = async (IEnumerable<Guid> guids) =>
            await Context.ShipmentItems
                .Where(x => guids.Contains(x.MeasureUnitGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(unitGuids, func);
    }

    public async Task EnsureByShipmentGuids(HashSet<Guid> shipmentGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await Context.ShipmentItems
                .Where(x => args.Contains(x.ShipmentGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(shipmentGuids, func);
    }

    public async Task EnsureByResourceGuids(HashSet<Guid> resourceGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await Context.ShipmentItems
                .Where(x => args.Contains(x.ResourceGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(resourceGuids, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: Context.ShipmentItems,
            entities: Collection.Values,
            createMapDelegate: entity => new Core.Infrastructure.Entities.ShipmentItem
            {
                Guid = entity.Guid,
                ShipmentGuid = entity.ShipmentGuid,
                ResourceGuid = entity.ResourceGuid,
                MeasureUnitGuid = entity.MeasureUnitGuid,
                Quantity = entity.Quantity
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.ShipmentGuid = entity.ShipmentGuid;
                dbEntity.ResourceGuid = entity.ResourceGuid;
                dbEntity.MeasureUnitGuid = entity.MeasureUnitGuid;
                dbEntity.Quantity = entity.Quantity;
            }
        );
    }

    protected override async Task<Dictionary<Guid, Item>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await Context.ShipmentItems
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
