namespace Directories.Infrastructure;
using Core.Infrastructure;
using Directories.Contracts;
using Directories.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Directories.Contracts.ResourceContract;

public class ResourceRepository : BaseRepository<Resource>, Resource.IRepository, IResourceProjectionRepository
{
    public class ResourceProjection : IResourceProjection
    {
        public Guid Guid { get; }
        public Conditions Condition { get; }

        public ResourceProjection(Resource resource)
        {
            Guid = resource.Guid;
            Condition = (Conditions)resource.Condition;
        }
    }
    IEnumerable<IResourceProjection> IResourceProjectionRepository.List => List.Select(x => new ResourceProjection(x));


    private Resource Restore(Core.Infrastructure.Entities.Resource resource) =>
        Resource.IRepository.Restore(resource.Guid, resource.Name, resource.Condition);
    public async Task EnsureByNames(HashSet<string> names)
    {
        var func = async (IEnumerable<string> args) =>
            await Context.Resources
                .Where(x => args.Contains(x.Name))
                .Where(x => !LoadedGuids.Contains(x.Guid))
                .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(names, func);
    }

    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: Context.Resources,
            entities: Collection.Values,
            createMapDelegate: entity => new Core.Infrastructure.Entities.Resource
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

    protected override async Task<Dictionary<Guid, Resource>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await Context.Resources
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
