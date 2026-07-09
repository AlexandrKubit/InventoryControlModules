namespace Balance.Domain.Entities;

using Common.Exceptions;
using Core.Domain;
using global::Balance.Domain.Data;
using System;
using System.Threading.Tasks;
using DCE = Directories.Contracts.Events;
using RCE = Receipt.Contracts.Events;
using SCE = Shipment.Contracts.Events;

/// <summary>
/// Баланс (свободный остаток на склада)
/// </summary>
public sealed class Balance : BaseEntity
{
    public interface IRepository : IBaseRepository<Balance>
    {
        protected static Balance Restore(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
            => new Balance(guid, resourceGuid, measureUnitGuid, quantity);

        public Task EnsureByResourceMeasureUnit(HashSet<(Guid ResourceGuid, Guid MeasureUnitGuid)> args);
        public Task EnsureByMeasureUnitGuids(HashSet<Guid> unitGuids);
        public Task EnsureByResourceGuids(HashSet<Guid> resourceGuids);
    }

    // подписываемся на события 
    static Balance()
    {
        RCE.OnReciptItemCreatedRange(OnReceiptItemCreatedRangeHandler);
        RCE.OnReciptItemUpdatedRange(OnReceiptItemUpdatedRangeHandler);
        RCE.OnReciptItemDeletedRange(OnReceiptItemDeletedRangeHandler);

        SCE.OnSignedRange(OnShipmentDocumentSignedRangeHandler);
        SCE.OnUnsignedRange(OnShipmentDocumentUnsignedRangeHandler);

        DCE.OnMeasureUnitDeletedRange(OnMeasureUnitDeletedRangeHandler);
        DCE.OnResourceDeletedRange(OnResourceDeletedRangeHandler);
    }

    public Guid ResourceGuid { get; }
    public Guid MeasureUnitGuid { get; }
    public decimal Quantity { get; private set; }

    private Balance(Guid guid, Guid resourceGuid, Guid measureUnitGuid, decimal quantity)
    {
        Guid = guid;
        ResourceGuid = resourceGuid;
        MeasureUnitGuid = measureUnitGuid;
        Quantity = quantity;
    }

    public record AddRangeToStockArg(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    private static async Task AddRangeToStock(List<AddRangeToStockArg> args, IBalanceData data)
    {
        var resourceMeasureUnits = args.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)).ToHashSet();
        await data.Balances.EnsureByResourceMeasureUnit(resourceMeasureUnits);

        foreach (var arg in args)
        {
            var balance = data.Balances.List.FirstOrDefault(x => x.MeasureUnitGuid == arg.MeasureUnitGuid && x.ResourceGuid == arg.ResourceGuid);
            if (balance != null)
            {
                balance.Quantity += arg.Quantity;
                balance.Update();
            }
            else
            {
                balance = new Balance(Guid.CreateVersion7(), arg.ResourceGuid, arg.MeasureUnitGuid, arg.Quantity);
                balance.Create();
                data.Balances.Add(balance);
            }
        }
    }

    public record RemoveRangeFromStockArg(Guid ResourceGuid, Guid MeasureUnitGuid, decimal Quantity);
    private static async Task RemoveRangeFromStock(List<RemoveRangeFromStockArg> args, IBalanceData data)
    {
        var resourceMeasureUnits = args.Select(x => (x.ResourceGuid, x.MeasureUnitGuid)).ToHashSet();
        await data.Balances.EnsureByResourceMeasureUnit(resourceMeasureUnits);

        foreach (var arg in args)
        {
            var balance = data.Balances.List.FirstOrDefault(x => x.MeasureUnitGuid == arg.MeasureUnitGuid && x.ResourceGuid == arg.ResourceGuid);
            if (balance != null)
            {
                if (balance.Quantity < arg.Quantity)
                    throw new DomainException("На складе не достаточно ресурсов");

                balance.Quantity -= arg.Quantity;
                if (balance.Quantity > 0)
                    balance.Update();
                else
                    balance.Remove();
            }
            else
            {
                throw new DomainException("На складе отсутствуют ресурсы");
            }
        }
    }


    private static async Task OnReceiptItemCreatedRangeHandler(RCE.ReciptItemCreatedRangeArg arg)
    {
        var args = arg.Items.Select(x => new AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList();
        await AddRangeToStock(args, (IBalanceData)arg.Data);
    }

    private static async Task OnReceiptItemUpdatedRangeHandler(RCE.ReciptItemUpdatedRangeArg arg)
    {
        var netChanges = new Dictionary<(Guid ResourceGuid, Guid MeasureUnitGuid), decimal>();
        var data = (IBalanceData)arg.Data;

        foreach (var change in arg.Changes)
        {
            var oldKey = (change.Old.ResourceGuid, change.Old.MeasureUnitGuid);
            var newKey = (change.New.ResourceGuid, change.New.MeasureUnitGuid);

            // Вычитаем старое количество
            if (netChanges.TryGetValue(oldKey, out var oldDelta))
                netChanges[oldKey] = oldDelta - change.Old.Quantity;
            else
                netChanges[oldKey] = -change.Old.Quantity;

            // Добавляем новое количество
            if (netChanges.TryGetValue(newKey, out var newDelta))
                netChanges[newKey] = newDelta + change.New.Quantity;
            else
                netChanges[newKey] = change.New.Quantity;
        }

        var toRemove = new List<RemoveRangeFromStockArg>();
        var toAdd = new List<AddRangeToStockArg>();

        foreach (var kv in netChanges)
        {
            if (kv.Value < 0)
            {
                toRemove.Add(new RemoveRangeFromStockArg(
                    kv.Key.ResourceGuid,
                    kv.Key.MeasureUnitGuid,
                    -kv.Value)); // передаём положительное количество для списания
            }
            else if (kv.Value > 0)
            {
                toAdd.Add(new AddRangeToStockArg(
                    kv.Key.ResourceGuid,
                    kv.Key.MeasureUnitGuid,
                    kv.Value));
            }
            // При kv.Value == 0 изменений нет, пропускаем
        }

        // Применяем изменения в правильном порядке (сначала списание, потом добавление)
        if (toRemove.Any())
            await RemoveRangeFromStock(toRemove, data);
        if (toAdd.Any())
            await AddRangeToStock(toAdd, data);
    }

    private static async Task OnReceiptItemDeletedRangeHandler(RCE.ReciptItemDeletedRangeArg arg)
    {
        var args = arg.Items.Select(x => new RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity)).ToList();
        await RemoveRangeFromStock(args, (IBalanceData)arg.Data);
    }

    private static async Task OnShipmentDocumentSignedRangeHandler(SCE.SignedRangeArg arg)
    {
        var guids = arg.DocumentGuids;
        var data = (IBalanceData)arg.Data;

        var items = await Shipment.Contracts.Lookup.GetShipmentItemsByShipmentGuidsAsync(guids, data);

        var removeRangeFromStockArgs = items
            .Select(x => new RemoveRangeFromStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await RemoveRangeFromStock(removeRangeFromStockArgs, (IBalanceData)arg.Data);
    }

    private static async Task OnShipmentDocumentUnsignedRangeHandler(SCE.UnsignedRangeArg arg)
    {
        var guids = arg.DocumentGuids;
        var data = (IBalanceData)arg.Data;

        var items = await Shipment.Contracts.Lookup.GetShipmentItemsByShipmentGuidsAsync(guids, data);

        var addRangeToStockArgs = items
            .Select(x => new AddRangeToStockArg(x.ResourceGuid, x.MeasureUnitGuid, x.Quantity))
            .ToList();

        await AddRangeToStock(addRangeToStockArgs, (IBalanceData)arg.Data);
    }

    private static async Task OnMeasureUnitDeletedRangeHandler(DCE.MeasureUnitDeletedRangeArg arg)
    {
        var data = (IBalanceData)arg.Data;
        await data.Balances.EnsureByMeasureUnitGuids(arg.Guids);

        if (data.Balances.List.Any(x => arg.Guids.Contains(x.MeasureUnitGuid)))
            throw new DomainException("Невозможно удалить единицу измерения т.к. она используется в складском остатке");
    }

    private static async Task OnResourceDeletedRangeHandler(DCE.ResourceDeletedRangeArg arg)
    {
        var data = (IBalanceData)arg.Data;
        await data.Balances.EnsureByResourceGuids(arg.Guids);

        if (data.Balances.List.Any(x => arg.Guids.Contains(x.ResourceGuid)))
            throw new DomainException("Невозможно удалить ресурс т.к. он используется в складском остатке");
    }
}
