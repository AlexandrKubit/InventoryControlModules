namespace Receipt.Domain.Entities;
using Common.Exceptions;
using Core.Domain;
using Receipt.Domain.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Directories.Contracts;
using I = Receipt.Contracts.ReciptItemContract;

/// <summary>
/// Ресурс накладной
/// </summary>
public sealed class Item : BaseEntity
{
    // при создании, изменении и удалении ресурсов поступления, необходимо изменять кол-во ресурсов на складе (баланс)
    // ресурсы на складе просто так нельзя удалять, добавлять или изменять на складе
    // поэтому баланс подписывается на эти события
    #region Events
    private static readonly DomainEvent<I.CreatedRangeArg> CreatedRange = new();
    private static readonly DomainEvent<I.UpdatedRangeArg> UpdatedRange = new();
    private static readonly DomainEvent<I.DeletedRangeArg> DeletedRange = new();
    #endregion

    public static void InitializeContracts()
    {
        I.OnCreatedRange = CreatedRange.Subscribe;
        I.OnUpdatedRange = UpdatedRange.Subscribe;
        I.OnDeletedRange = DeletedRange.Subscribe;
    }

    static Item()
    {
        MeasureUnitContract.OnDeletedRange(OnMeasureUnitDeletedRangeHandler);
        ResourceContract.OnDeletedRange(OnResourceRangeHandler);
    }

    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, receiptGuid, resourceGuid, measureUnitGuid, quantity);

        public abstract Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids);
        public abstract Task EnsureByResourceGuids(HashSet<Guid> resourceGuids);
        public abstract Task EnsureByReceiptGuids(HashSet<Guid> receiptGuids);
    }


    public Guid ReceiptGuid { get; }
    public Guid ResourceGuid { get; private set; }
    public Guid MeasureUnitGuid { get; private set; }
    public decimal Quantity { get; private set; }

    private Item(Guid guid, Guid receiptGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ReceiptGuid = receiptGuid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record CreateArg(Guid ReceiptGuid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task<List<Item>> CreateRange(List<CreateArg> args, IReceiptData data)
    {
        List<Item> items = [];

        var resourceGuids = args.Select(x => x.ResourceGuid).ToHashSet();
        var unitGuids = args.Select(x => x.MeasureUnitGuid).ToHashSet();

        await data.ResourceProjections.EnsureByGuids(resourceGuids);
        await data.MeasureUnitProjections.EnsureByGuids(unitGuids);

        foreach (var arg in args)
        {
            var resource = data.ResourceProjections.List.FirstOrDefault(x => x.Guid == arg.ResourceGuid);
            if (resource == null || resource.Condition == ResourceContract.Conditions.Archive)
                throw new DomainException("Ресурс удален или переведен в архив");

            var unit = data.MeasureUnitProjections.List.FirstOrDefault(x => x.Guid == arg.MeasureUnitGuid);
            if (unit == null || unit.Condition == MeasureUnitContract.Conditions.Archive)
                throw new DomainException("Единица измерения удалена или переведена в архив");

            var item = new Item(Guid.CreateVersion7(), arg.ReceiptGuid, arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            item.Create();
            data.ReceiptItems.Add(item);
            items.Add(item);
        }

        await CreatedRange.Invoke(new I.CreatedRangeArg(
            items.Select(x => new I.ItemData(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList(),
            data
        ));
        return items;
    }

    public record UpdateArg(Guid Guid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task UpdateRange(List<UpdateArg> args, IReceiptData data)
    {
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.ReceiptItems.EnsureByGuids(guids);
        var items = data.ReceiptItems.List.Where(x => guids.Contains(x.Guid)).ToList();

        List<(I.ItemData Old, I.ItemData New)> сhanges = [];

        foreach (var item in items)
        {
            var arg = args.First(x => x.Guid == item.Guid);

            I.ItemData old = new(item.ResourceGuid, item.MeasureUnitGuid, item.Quantity);
            I.ItemData _new = new(arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            сhanges.Add((old, _new));

            item.ResourceGuid = arg.ResourceGuid;
            item.MeasureUnitGuid = arg.MeasureUnitGuid;
            item.Quantity = arg.Quantity;
            item.Update();
        }

        await UpdatedRange.Invoke(new I.UpdatedRangeArg(сhanges, data));
    }


    public static async Task DeleteRange(HashSet<Guid> guids, IReceiptData data)
    {
        await data.ReceiptItems.EnsureByGuids(guids);
        var items = data.ReceiptItems.List.Where(x => guids.Contains(x.Guid)).ToList();

        foreach (var item in items)
            item.Remove();

        await DeletedRange.Invoke(new I.DeletedRangeArg(
            items.Select(x => new I.ItemData(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList(), 
            data
        ));
    }

    private static async Task OnMeasureUnitDeletedRangeHandler(MeasureUnitContract.DeletedRangeArg arg)
    {
        var data = (IReceiptData)arg.Data;
        await data.ReceiptItems.EnsureByMeasureUnitGuids(arg.Guids);

        if (data.ReceiptItems.List.Any(x => arg.Guids.Contains(x.MeasureUnitGuid)))
            throw new DomainException("Невозможно удалить единицу измерения т.к. она используется в поступлениях");
    }

    private static async Task OnResourceRangeHandler(ResourceContract.DeletedRangeArg arg)
    {
        var data = (IReceiptData)arg.Data;
        await data.ReceiptItems.EnsureByResourceGuids(arg.Guids);

        if (data.ReceiptItems.List.Any(x => arg.Guids.Contains(x.MeasureUnitGuid)))
            throw new DomainException("Невозможно удалить ресурс т.к. он используется в поступлениях");
    }
}