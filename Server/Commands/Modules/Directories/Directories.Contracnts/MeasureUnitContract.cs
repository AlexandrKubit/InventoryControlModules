namespace Directories.Contracts;

using Core.Domain;
using Directories.Domain.Entities;

public static class MeasureUnitContract
{
    static MeasureUnitContract()
    {
        MeasureUnit.OnDeletedRange(OnMeasureUnitDeleteRangeHandler);
    }

    public record DeletedRangeArg(HashSet<Guid> Guids, IData Data);
    public static event Func<DeletedRangeArg, Task> DeletedRange = _ => Task.CompletedTask;

    private static async Task OnMeasureUnitDeleteRangeHandler(MeasureUnit.DeletedRangeArg args)
    {
        await DeletedRange.Invoke(new DeletedRangeArg(args.Guids, args.Data));
    }

    public interface IMeasureUnitProjection
    {
        Guid Guid { get; }
        Conditions Condition { get; }
    }

    public enum Conditions
    {
        Work = 1,
        Archive = 2
    }

    public interface IMeasureUnitProjectionRepository
    {
        IEnumerable<IMeasureUnitProjection> List { get; }
        Task EnsureByGuids(HashSet<Guid> guids);
    }
}
