namespace Receipt.Contracts;

using Core.Domain;
using I = Domain.Entities.Item;

public static class Events
{
    static Events()
    {
        I.OnCreatedRange(OnReciptItemCreatedRangeHandler);
        I.OnUpdatedRange(OnReciptItemUpdatedRangeHandler);
        I.OnDeletedRange(OnReciptItemDeletedRangeHandler);
    }
    public record ItemData(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);

    public record ReciptItemCreatedRangeArg(List<ItemData> Items, IData Data);
    private static readonly DomainEvent<ReciptItemCreatedRangeArg> ReciptItemCreatedRange = new();
    public static Action<Func<ReciptItemCreatedRangeArg, Task>> OnReciptItemCreatedRange 
        => ReciptItemCreatedRange.Subscribe;

    public record ReciptItemUpdatedRangeArg(List<(ItemData Old, ItemData New)> Changes, IData Data);
    private static readonly DomainEvent<ReciptItemUpdatedRangeArg> ReciptItemUpdatedRange = new();
    public static Action<Func<ReciptItemUpdatedRangeArg, Task>> OnReciptItemUpdatedRange 
        => ReciptItemUpdatedRange.Subscribe;

    public record ReciptItemDeletedRangeArg(List<ItemData> Items, IData Data);
    private static readonly DomainEvent<ReciptItemDeletedRangeArg> ReciptItemDeletedRange = new();
    public static Action<Func<ReciptItemDeletedRangeArg, Task>> OnReciptItemDeletedRange 
        => ReciptItemDeletedRange.Subscribe;

    private static async Task OnReciptItemCreatedRangeHandler(I.CreatedRangeArg args)
    {
        var items = args.Items
            .Select(x => new ItemData(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await ReciptItemCreatedRange.Invoke(new ReciptItemCreatedRangeArg(items, args.Data));
    }

    private static async Task OnReciptItemUpdatedRangeHandler(I.UpdatedRangeArg args)
    {
        var items = args.Changes
            .Select(x => 
            (
                new ItemData(x.Old.ResourceGuid, x.Old.MeasureUnitGuid, x.Old.Quantity),
                new ItemData(x.New.ResourceGuid, x.New.MeasureUnitGuid, x.New.Quantity)
            )).ToList();

        await ReciptItemUpdatedRange.Invoke(new ReciptItemUpdatedRangeArg(items, args.Data));
    }

    private static async Task OnReciptItemDeletedRangeHandler(I.DeletedRangeArg args)
    {
        var items = args.Items
            .Select(x => new ItemData(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await ReciptItemDeletedRange.Invoke(new ReciptItemDeletedRangeArg(items, args.Data));
    }
}
