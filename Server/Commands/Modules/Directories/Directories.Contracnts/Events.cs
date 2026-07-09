namespace Directories.Contracts;

using Core.Domain;
using Directories.Domain.Entities;

public static class Events
{
    static Events()
    {
        Client.OnDeletedRange(OnClientDeleteRangeHandler);
        Resource.OnDeletedRange(OnResourceDeleteRangeHandler);
        MeasureUnit.OnDeletedRange(OnMeasureUnitDeleteRangeHandler);
    }

    #region Client
    public record ClientDeletedRangeArg(HashSet<Guid> Guids, IData Data);
    private static DomainEvent<ClientDeletedRangeArg> ClientDeletedRange = new();
    public static Action<Func<ClientDeletedRangeArg, Task>> OnClientDeletedRange
        => ClientDeletedRange.Subscribe;

    private static async Task OnClientDeleteRangeHandler(Client.DeletedRangeArg args)
    {
        await ClientDeletedRange.Invoke(new ClientDeletedRangeArg(args.Guids, args.Data));
    }
    #endregion

    #region Resource
    public record ResourceDeletedRangeArg(HashSet<Guid> Guids, IData Data);
    private static DomainEvent<ResourceDeletedRangeArg> ResourceDeletedRange = new();
    public static Action<Func<ResourceDeletedRangeArg, Task>> OnResourceDeletedRange
        => ResourceDeletedRange.Subscribe;

    private static async Task OnResourceDeleteRangeHandler(Resource.DeletedRangeArg args)
    {
        await ResourceDeletedRange.Invoke(new ResourceDeletedRangeArg(args.Guids, args.Data));
    }
    #endregion

    #region MeasureUnit
    public record MeasureUnitDeletedRangeArg(HashSet<Guid> Guids, IData Data);
    private static DomainEvent<MeasureUnitDeletedRangeArg> MeasureUnitDeletedRange = new();
    public static Action<Func<MeasureUnitDeletedRangeArg, Task>> OnMeasureUnitDeletedRange
        => MeasureUnitDeletedRange.Subscribe;

    private static async Task OnMeasureUnitDeleteRangeHandler(MeasureUnit.DeletedRangeArg args)
    {
        await MeasureUnitDeletedRange.Invoke(new MeasureUnitDeletedRangeArg(args.Guids, args.Data));
    }
    #endregion
}
