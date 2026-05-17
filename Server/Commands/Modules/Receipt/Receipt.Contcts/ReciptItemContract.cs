namespace Receipt.Contracts;
using Core.Domain;

public class ReciptItemContract
{
    public record ItemData(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public record CreatedRangeArg(List<ItemData> Items, IData Data);
    public static Action<Func<CreatedRangeArg, Task>> OnCreatedRange { get; set; }

    public record UpdatedRangeArg(List<(ItemData Old, ItemData New)> Changes, IData Data);
    public static Action<Func<UpdatedRangeArg, Task>> OnUpdatedRange { get; set; }

    public record DeletedRangeArg(List<ItemData> Items, IData Data);
    public static Action<Func<DeletedRangeArg, Task>> OnDeletedRange { get; set; }
}
