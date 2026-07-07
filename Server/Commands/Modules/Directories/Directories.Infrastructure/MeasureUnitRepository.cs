namespace Directories.Infrastructure;

using Core.Infrastructure;
using Directories.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Directories.Contracts.MeasureUnitContract;

public class MeasureUnitRepository : BaseRepository<MeasureUnit>, MeasureUnit.IRepository, IMeasureUnitProjectionRepository
{
    public class MeasureUnitProjection : IMeasureUnitProjection
    {
        public Guid Guid { get; }
        public Conditions Condition { get; }

        public MeasureUnitProjection(MeasureUnit unit)
        {
            Guid = unit.Guid;
            Condition = (Conditions)unit.Condition;
        }
    }
    IEnumerable<IMeasureUnitProjection> IMeasureUnitProjectionRepository.List => List.Select(x => new MeasureUnitProjection(x));

    private MeasureUnit Restore(Core.Infrastructure.Entities.MeasureUnit unit) =>
        MeasureUnit.IRepository.Restore(unit.Guid, unit.Name, unit.Condition);

    public async Task EnsureByNames(HashSet<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await Context.MeasureUnits
                .Where(x => args.Contains(x.Name))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(names, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: Context.MeasureUnits,
            entities: Collection.Values,
            createMapDelegate: entity => new Core.Infrastructure.Entities.MeasureUnit
            {
                Guid = entity.Guid,
                Name = entity.Name,
                Condition = entity.Condition
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Name = entity.Name;
                dbEntity.Condition = entity.Condition;
            }
        );
    }

    protected override async Task<Dictionary<Guid, MeasureUnit>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await Context.MeasureUnits
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
