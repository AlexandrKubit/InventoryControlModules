namespace Directories.Contracts;

using Core.Domain;
using Directories.Domain.Entities;

public static class ClientContract
{
    static ClientContract()
    {
        Client.OnDeletedRange(OnClientDeleteRangeHandler);
    }

    public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
    public static event Func<DeletedRangeArg, Task> DeletedRange = _ => Task.CompletedTask;

    private static async Task OnClientDeleteRangeHandler(Client.DeletedRangeArg args)
    {
        await DeletedRange.Invoke(new DeletedRangeArg(args.Guids, args.Data));
    }

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
