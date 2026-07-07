namespace Directories.Contracts;

using Core.Domain;
using Directories.Domain.Entities;

public static class ResourceContract
{
    static ResourceContract()
    {
        Resource.OnDeletedRange(OnResourceDeleteRangeHandler);
    }

    public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
    public static event Func<DeletedRangeArg, Task> DeletedRange = _ => Task.CompletedTask;

    private static async Task OnResourceDeleteRangeHandler(Resource.DeletedRangeArg args)
    {
        await DeletedRange.Invoke(new DeletedRangeArg(args.Guids, args.Data));
    }

    public interface IResourceProjection
    {
        Guid Guid { get; }
        Conditions Condition { get; }
    }

    public enum Conditions
    {
        Work = 1,
        Archive = 2
    }

    public interface IResourceProjectionRepository
    {
        IEnumerable<IResourceProjection> List { get; }
        Task EnsureByGuids(HashSet<Guid> guids);
    }
}
