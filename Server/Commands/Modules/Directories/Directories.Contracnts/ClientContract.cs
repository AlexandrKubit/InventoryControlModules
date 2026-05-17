namespace Directories.Contracts;
using Core.Domain;

public static class ClientContract
{
    public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
    public static Action<Func<DeletedRangeArg, Task>> OnDeletedRange { get; set; }

    public interface IClientProjection
    {
        Guid Guid { get; }
        Conditions Condition { get; }
    }

    public enum Conditions
    {
        Work = 1,
        Archive = 2
    }

    public interface IClientProjectionRepository
    {
        IEnumerable<IClientProjection> List { get; }
        Task EnsureByGuids(HashSet<Guid> guids);
    }
}
