using Balance.Domain.Data;
using Core.Domain;
using Directories.Contracts;
using Directories.Domain.Data;
using Receipt.Domain.Data;
using Shipment.Contracts;
using Shipment.Domain.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using TestProject.Infrastructure;
using TestProject.Infrastructure.Repositories;

namespace Tests.Infrastructure;

internal class TestData : IData, IDirectoriesData, IReceiptData, IShipmentData, IBalanceData
{
    static TestData()
    {
        var type = typeof(Core.Domain.BaseEntity);

        var balanceAsm = Assembly.GetAssembly(typeof(Balance.Domain.Data.IBalanceData));
        var types = balanceAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => RuntimeHelpers.RunClassConstructor(t.TypeHandle));

        var directoriesAsm = Assembly.GetAssembly(typeof(Directories.Domain.Data.IDirectoriesData));
        types = balanceAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => RuntimeHelpers.RunClassConstructor(t.TypeHandle));

        var receiptAsm = Assembly.GetAssembly(typeof(Receipt.Domain.Data.IReceiptData));
        types = receiptAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => RuntimeHelpers.RunClassConstructor(t.TypeHandle));

        var shipmentAsm = Assembly.GetAssembly(typeof(Shipment.Domain.Data.IShipmentData));
        types = shipmentAsm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(type)).ToList();
        types.ForEach(t => RuntimeHelpers.RunClassConstructor(t.TypeHandle));
    }

    private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

    public TestData()
    {
        // Автоматически находим и создаем все тестовые репозитории через рефлексию
        var testRepositoryTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(TestBaseRepository<>))
            .ToList();

        foreach (var repoType in testRepositoryTypes)
        {
            var repositoryInstance = Activator.CreateInstance(repoType)
                ?? throw new Exception($"Не удалось найти репозиторий для типа {repoType}");
            repositories[repoType] = repositoryInstance;
        }
    }

    private T Get<T>() where T : class
    {
        var type = typeof(T);
        if (repositories.TryGetValue(type, out var repository))
        {
            return (T)repository;
        }
        throw new Exception($"Не удалось найти репозиторий для типа {type.Name}");
    }

    public void LoadData()
    {
        foreach (var repo in repositories.Values)
        {
            var initDataMethod = repo.GetType().GetMethod("InitData");
            initDataMethod?.Invoke(repo, null);
        }
    }

    // Явная реализация интерфейса IData
    public Directories.Domain.Entities.Client.IRepository Clients => Get<TestClientRepository>();
    public Directories.Domain.Entities.MeasureUnit.IRepository MeasureUnits { get; }
    public Directories.Domain.Entities.Resource.IRepository Resources { get; }
    public Balance.Domain.Entities.Balance.IRepository Balances => Get<TestBalanceRepository>();
    public Receipt.Domain.Entities.Document.IRepository Receipts { get; }
    public Receipt.Domain.Entities.Item.IRepository ReceiptItems => Get<TestReceiptItemRepository>();
    public Shipment.Domain.Entities.Document.IRepository Shipments { get; }
    public Shipment.Domain.Entities.Item.IRepository ShipmentItems { get; }
    public MeasureUnitContract.IMeasureUnitProjectionRepository MeasureUnitProjections { get; }
    public ResourceContract.IResourceProjectionRepository ResourceProjections { get; }
    public ClientContract.IClientProjectionRepository ClientProjections => Get<TestClientRepository>();
    public ShipmentItemContract.IShipmentItemProjectionRepository ShipmentItemProjections { get; }
}