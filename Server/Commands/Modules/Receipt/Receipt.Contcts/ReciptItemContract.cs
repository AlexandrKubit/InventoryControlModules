namespace Receipt.Contracts;

using Core.Domain;
using I = Domain.Entities.Item;

public static class ReciptItemContract
{
    static ReciptItemContract()
    {
        I.OnCreatedRange(OnReciptItemCreatedRangeHandler);
        I.OnUpdatedRange(OnReciptItemUpdatedRangeHandler);
        I.OnDeletedRange(OnReciptItemDeletedRangeHandler);
    }
    public record ItemData(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);


    public record CreatedRangeArg(List<ItemData> Items, IData Data);
    public static event Func<CreatedRangeArg, Task> CreatedRange = _ => Task.CompletedTask;

    public record UpdatedRangeArg(List<(ItemData Old, ItemData New)> Changes, IData Data);
    public static event Func<UpdatedRangeArg, Task> UpdatedRange = _ => Task.CompletedTask;

    public record DeletedRangeArg(List<ItemData> Items, IData Data);
    public static event Func<DeletedRangeArg, Task> DeletedRange = _ => Task.CompletedTask;

    private static async Task OnReciptItemCreatedRangeHandler(I.CreatedRangeArg args)
    {
        var items = args.Items
            .Select(x => new ItemData(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await CreatedRange.Invoke(new CreatedRangeArg(items, args.Data));
    }

    private static async Task OnReciptItemUpdatedRangeHandler(I.UpdatedRangeArg args)
    {
        var items = args.Changes
            .Select(x => 
            (
                new ItemData(x.Old.ResourceGuid, x.Old.MeasureUnitGuid, x.Old.Quantity),
                new ItemData(x.New.ResourceGuid, x.New.MeasureUnitGuid, x.New.Quantity)
            )).ToList();

        await UpdatedRange.Invoke(new UpdatedRangeArg(items, args.Data));
    }

    private static async Task OnReciptItemDeletedRangeHandler(I.DeletedRangeArg args)
    {
        var items = args.Items
            .Select(x => new ItemData(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await DeletedRange.Invoke(new DeletedRangeArg(items, args.Data));
    }

}
