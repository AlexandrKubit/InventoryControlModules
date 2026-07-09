namespace Shipment.Domain.Entities;

using Common.Exceptions;
using Core.Domain;
using DCE = Directories.Contracts.Events;
using Shipment.Domain.Data;
using System.Threading.Tasks;

/// <summary>
/// Ресурс отгрузки
/// </summary>
public sealed class Item : BaseEntity
{
    static Item()
    {
        DCE.OnResourceDeletedRange(OnResourceDeletedRangeHandler);
        DCE.OnMeasureUnitDeletedRange(OnMeasureUnitDeletedRangeHandler);
    }

    public interface IRepository : IBaseRepository<Item>
    {
        protected static Item Restore(Guid guid, Guid shipmentGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Item(guid, shipmentGuid, resourceGuid, measureUnitGuid, quantity);

        public abstract Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids);
        public abstract Task EnsureByResourceGuids(HashSet<Guid> resourceGuids);
        public abstract Task EnsureByShipmentGuids(HashSet<Guid> shipmentGuids);
    }

    public Guid ShipmentGuid { get; }
    public Guid ResourceGuid { get; private set; }
    public Guid MeasureUnitGuid { get; private set; }
    public decimal Quantity { get; private set; }

    private Item(Guid guid, Guid shipmentGuid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ShipmentGuid = shipmentGuid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record CreateArg(Guid ShipmentGuid, Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    public static async Task<List<Item>> CreateRange(List<CreateArg> args, IShipmentData data)
    {
        var shipmentGuids = args.Select(x => x.ShipmentGuid).ToHashSet();
        await data.Shipments.EnsureByGuids(shipmentGuids);

        if (data.Shipments.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно добавить ресурс в подписанную отгрузку");

        List<Item> items = [];

        foreach (var arg in args)
        {
            var item = new Item(Guid.CreateVersion7(), arg.ShipmentGuid, arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
            item.Create();
            data.ShipmentItems.Add(item);
            items.Add(item);
        }

        return items;
    }

    public record UpdateArg(
        Guid Guid,
        Guid ResourceGuid,
        Guid MeasureUnitGuid,
        decimal Quantity
    );
    public static async Task UpdateRange(List<UpdateArg> args, IShipmentData data)
    {
        // Подготавливаем все изменяемые позиции по их идентификаторам
        var guids = args.Select(x => x.Guid).ToHashSet();
        await data.ShipmentItems.EnsureByGuids(guids);
        var items = data.ShipmentItems.List
            .Where(x => guids.Contains(x.Guid)).ToList();

        // Определяем, к каким отгрузкам относятся эти позиции
        var shipmentGuids = items.Select(x => x.ShipmentGuid).ToHashSet();
        await data.Shipments.EnsureByGuids(shipmentGuids);

        // Проверяем бизнес-правило: нельзя менять позиции в подписанной отгрузке
        var shipments = data.Shipments.List
            .Where(x => shipmentGuids.Contains(x.Guid));

        if (shipments.Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно изменить ресурс в подписанной отгрузке");

        // Применяем изменения к каждой позиции
        foreach (var item in items)
        {
            var arg = args.First(x => x.Guid == item.Guid);
            item.ResourceGuid = arg.ResourceGuid;
            item.MeasureUnitGuid = arg.MeasureUnitGuid;
            item.Quantity = arg.Quantity;
            item.Update();
        }
    }

    public static async Task DeleteRange(HashSet<Guid> guids, IShipmentData data)
    {
        await data.ShipmentItems.EnsureByGuids(guids);
        var items = data.ShipmentItems.List.Where(x => guids.Contains(x.Guid)).ToList();

        var shipmentGuids = items.Select(x => x.ShipmentGuid).ToHashSet();
        await data.Shipments.EnsureByGuids(shipmentGuids);

        if (data.Shipments.List.Where(x => shipmentGuids.Contains(x.Guid)).Any(x => x.Condition == Document.Conditions.Signed))
            throw new DomainException("Невозможно удалить ресурс из подписанной отгрузки");

        foreach (var item in items)
            item.Remove();

        await data.ShipmentItems.EnsureByShipmentGuids(shipmentGuids);

        foreach (var sg in shipmentGuids)
        {
            if (!data.ShipmentItems.List.Any(x => x.ShipmentGuid == sg))
                throw new DomainException("Невозможно удалить все ресурсы из отгрузки");
        }
    }

    private static async Task OnResourceDeletedRangeHandler(DCE.ResourceDeletedRangeArg arg)
    {
        var data = (IShipmentData)arg.Data;
        await data.ShipmentItems.EnsureByResourceGuids(arg.Guids);

        if (data.ShipmentItems.List.Any(x => arg.Guids.Contains(x.ResourceGuid)))
            throw new DomainException("Невозможно удалить ресурс т.к. он используется в отгрузке");
    }

    private static async Task OnMeasureUnitDeletedRangeHandler(DCE.MeasureUnitDeletedRangeArg arg)
    {
        var data = (IShipmentData)arg.Data;
        await data.ShipmentItems.EnsureByMeasureUnitGuids(arg.Guids);

        if (data.ShipmentItems.List.Any(x => arg.Guids.Contains(x.MeasureUnitGuid)))
            throw new DomainException("Невозможно удалить единицу измерения т.к. она используется в отгрузке");
    }
}
