namespace Directories.Contracts;
using Core.Domain;

public static class ResourceContract
{
    public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
    public static Action<Func<DeletedRangeArg, Task>> OnDeletedRange { get; set; }

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
