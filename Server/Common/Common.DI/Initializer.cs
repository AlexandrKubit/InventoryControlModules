namespace Common.DI;
using App.Commands.Base;
using App.Queries.Base;
using Common.Requests;
using System.Reflection;


public static class Initializer
{
    // регистрация хендлеров/валидаторов
    public static void InitializeMediator()
    {
        Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(CommandDispatcher)));
        Mediator.RegisterHandlersAndValidators(Assembly.GetAssembly(typeof(QueryDispatcher)));
    }

    // вызов статических конструкторов
    public static void InitializeDomain()
    {
        var type = typeof(Core.Domain.BaseEntity);

        var balanceAsm = Assembly.GetAssembly(typeof(Balance.Domain.Data.IBalanceData));
        var types = balanceAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle));

        var directoriesAsm = Assembly.GetAssembly(typeof(Directories.Domain.Data.IDirectoriesData));
        types = balanceAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle));

        var receiptAsm = Assembly.GetAssembly(typeof(Receipt.Domain.Data.IReceiptData));
        types = receiptAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle));

        var shipmentAsm = Assembly.GetAssembly(typeof(Shipment.Domain.Data.IShipmentData));
        types = shipmentAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle));
    }
}
