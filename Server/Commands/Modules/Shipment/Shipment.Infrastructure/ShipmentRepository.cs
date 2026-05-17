namespace Shipment.Infrastructure;
using Core.Infrastructure;
using Shipment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ShipmentRepository : BaseRepository<Document>, Document.IRepository
{
    private Document Restore(Core.Infrastructure.Entities.Shipment shipment) =>
        Document.IRepository.Restore(shipment.Guid, shipment.Number, shipment.ClientGuid, shipment.Date, shipment.Condition);

    public async Task EnsureByClients(HashSet<Guid> clientGuids)
    {
        var func = async (IEnumerable<Guid> args) =>
            await Context.Shipments
                .Where(x => args.Contains(x.ClientGuid))
				.Where(x => !LoadedGuids.Contains(x.Guid))
				.ToDictionaryAsync(x => x.Guid, x => Restore(x));

		await LoadWithCacheAsync(clientGuids, func);
    }

    public async Task EnsureByNumbers(HashSet<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
            await Context.Shipments
                .Where(x => args.Contains(x.Number))
				.Where(x => !LoadedGuids.Contains(x.Guid))
				.ToDictionaryAsync(x => x.Guid, x => Restore(x));

		await LoadWithCacheAsync(numbers, func);
    }


    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: Context.Shipments,
            entities: Collection.Values,
            createMapDelegate: entity => new Core.Infrastructure.Entities.Shipment
            {
                Guid = entity.Guid,
                Number = entity.Number,
                Date = entity.Date.ToUniversalTime(),
                ClientGuid = entity.ClientGuid,
                Condition = entity.Condition
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Number = entity.Number;
                dbEntity.Date = entity.Date.ToUniversalTime();
                dbEntity.ClientGuid = entity.ClientGuid;
                dbEntity.Condition = entity.Condition;
            }
        );
    }

    protected override async Task<Dictionary<Guid,Document>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await Context.Shipments
			.Where(x => guids.Contains(x.Guid))
			.ToDictionaryAsync(x => x.Guid, x => Restore(x));
	}
}
