namespace Balance.Infrastructure;
using Balance.Domain.Entities;
using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using E = Core.Infrastructure.Entities;

public class BalanceRepository : BaseRepository<Balance>, Balance.IRepository
{
    private Balance Restore(E.Balance balance)
    {
        return Balance.IRepository.Restore(balance.Guid, balance.ResourceGuid, balance.MeasureUnitGuid, balance.Quantity);
    }


    public async Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids)
    {
        var func = async (HashSet<Guid> guids) =>
            await Context.Balances
                .Where(x => guids.Contains(x.MeasureUnitGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(unitGuids, func);
    }

    public async Task EnsureByResourceGuids(HashSet<Guid> resourceGuids)
    {
        var func = async (HashSet<Guid> guids) =>
             await Context.Balances
                .Where(x => guids.Contains(x.ResourceGuid))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(resourceGuids, func);
    }

    public async Task EnsureByResourceMeasureUnit(HashSet<(Guid ResourceGuid, Guid MeasureUnitGuid)> args)
    {
        var compositeKeys = args.Select(a => $"{a.ResourceGuid}:{a.MeasureUnitGuid}").ToHashSet();

        var func = async (IEnumerable<string> args) =>
            await Context.Balances
                .Where(x => args.Contains(x.ResourceGuid.ToString() + ":" + x.MeasureUnitGuid.ToString()))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(compositeKeys, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: Context.Balances,
            entities: Collection.Values,
            createMapDelegate: entity => new E.Balance
            {
                Guid = entity.Guid,
                MeasureUnitGuid = entity.MeasureUnitGuid,
                ResourceGuid = entity.ResourceGuid,
                Quantity = entity.Quantity
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.MeasureUnitGuid = entity.MeasureUnitGuid;
                dbEntity.ResourceGuid = entity.ResourceGuid;
                dbEntity.Quantity = entity.Quantity;
            }
        );
    }

    protected override async Task<Dictionary<Guid, Balance>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await Context.Balances
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
