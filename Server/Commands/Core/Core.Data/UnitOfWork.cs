namespace Core.Data;

using App.Commands.Base;
using Balance.Domain.Data;
using Balance.Infrastructure;
using Core.Domain;
using Core.Infrastructure;
using Directories.Domain.Data;
using Directories.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Receipt.Domain.Data;
using Receipt.Infrastructure;
using Shipment.Domain.Data;
using Shipment.Infrastructure;
using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Единица работы (Unit of Work) — главный координатор инфраструктуры.
/// Объединяет доступ к репозиториям и управление транзакцией.
/// Живёт в рамках одного сценария (запроса), обеспечивая изоляцию и атомарность.
/// </summary>
public sealed class UnitOfWork : IData, IUnitOfWork, IDirectoriesData, IReceiptData, IShipmentData, IBalanceData
{
    private Context? context;
    private IServiceProvider provider;
    private IDbContextTransaction? transaction;
    private string connectionString;
    private Dictionary<Type, BaseRepository> repositories = new();

    public UnitOfWork(IServiceProvider provider, string connectionString)
    {
        this.provider = provider;
        this.connectionString = connectionString;
    }

    /// <summary>
    /// Ленивое получение репозитория с кэшированием в рамках UoW.
    /// Использует ActivatorUtilities, чтобы репозиторий получил все зависимости (в том числе Context).
    /// </summary>
    private T Get<T>() where T : BaseRepository
    {
        if (context == null)
            throw new Exception("Контекст не проинициализирован");

        var type = typeof(T);
        if (!repositories.TryGetValue(type, out var repository))
        {
            repository = ActivatorUtilities.CreateInstance<T>(provider)
                ?? throw new Exception($"Не удалось создать экземпляр репозитория {type.Name}");

            repository.SetContext(context);
            repositories[type] = repository;
        }

        return (T)repository;
    }

    /// <summary>
    /// Инициализация UoW: создание контекста EF, открытие соединения и начало транзакции.
    /// Вызывается один раз перед началом работы со сценарием.
    /// </summary>
    public async Task InitializeAsync(System.Data.IsolationLevel isolationLevel)
    {
        var options = new DbContextOptionsBuilder<Context>()
            .UseNpgsql(connectionString)
            .Options;

        context = new Context(options);
        await context.Database.OpenConnectionAsync();
        transaction = await context.Database.BeginTransactionAsync(isolationLevel);
        repositories = new();
    }

    /// <summary>
    /// Фиксация изменений:
    /// 1. Каждый репозиторий синхронизирует свои коллекции с DbSet (создание/обновление/удаление).
    /// 2. Сохранение изменений в БД.
    /// 3. Коммит транзакции.
    /// После коммита транзакция уничтожается.
    /// </summary>
    public async Task CommitAsync()
    {
        if (context == null)
            throw new Exception("Контекст не проинициализирован");

        if (transaction == null)
            throw new Exception("Транзакция не проинициализирована");

        foreach (var item in repositories)
            item.Value.Commit();

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        await transaction.DisposeAsync(); // освобождаем транзакцию
        transaction = null;

        // После успешной фиксации транзакции можно безопасно выполнить
        // отложенные инфраструктурные действия: отправку уведомлений, 
        // публикацию событий во внешние шины, вызовы API и т.п.
        // Все они будут выполнены только после гарантированного сохранения данных
    }

    /// <summary>
    /// Откат изменений. Освобождает транзакцию и контекст.
    /// </summary>
    public async Task RollbackAsync()
    {
        if (transaction != null)
        {
            await transaction.RollbackAsync();
            await transaction.DisposeAsync();
            transaction = null;
        }
        if (context != null)
        {
            await context.DisposeAsync();
            context = null;
        }
    }

    /// <summary>
    /// Проверка, является ли исключение 
    /// 40P01 (deadlock) или 40001 (serialization failure).
    /// Используется для реализации стратегий повторных попыток.
    /// </summary>
    public bool IsTransientConcurrencyException(Exception? exception)
    {
        if (exception == null)
            return false;

        if (exception is PostgresException postgresEx
            && (postgresEx.SqlState == "40P01" || postgresEx.SqlState == "40001"))
            return true;

        if (exception is DbUpdateException dbUpdateEx)
            return IsTransientConcurrencyException(dbUpdateEx.InnerException);

        if (exception.InnerException != null)
            return IsTransientConcurrencyException(exception.InnerException);

        return false;
    }


    public async Task AcquireLock(Type entityType, string key)
    {
        if (context == null)
            throw new Exception("Контекст не проинициализирован");

        // Используем SHA256 для получения 64-битного хеша
        // Вероятность коллизии 64-битного хеша на несколько порядков ниже, 
        // чем у 32-битного, что делает этот метод достаточно надежным 
        // для большинства бизнес-приложений
        var fullKey = $"{entityType.FullName}:{key}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(fullKey));
        var key1 = BitConverter.ToInt32(hashBytes, 0); // Биты 0-31
        var key2 = BitConverter.ToInt32(hashBytes, 4); // Биты 32-63

        await context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(@key1, @key2)",
            new NpgsqlParameter("key1", key1),
            new NpgsqlParameter("key2", key2));
    }


    // Доступ к конкретным репозиториям через интерфейс IData.
    // Репозитории создаются лениво и кэшируются.
    Directories.Domain.Entities.Client.IRepository IDirectoriesData.Clients => Get<ClientRepository>();
    Directories.Domain.Entities.MeasureUnit.IRepository IDirectoriesData.MeasureUnits => Get<MeasureUnitRepository>();
    Directories.Domain.Entities.Resource.IRepository IDirectoriesData.Resources => Get<ResourceRepository>();
    Balance.Domain.Entities.Balance.IRepository IBalanceData.Balances => Get<BalanceRepository>();
    Receipt.Domain.Entities.Document.IRepository IReceiptData.Receipts => Get<ReceiptRepository>();
    Receipt.Domain.Entities.Item.IRepository IReceiptData.ReceiptItems => Get<ReceiptItemRepository>();
    Shipment.Domain.Entities.Document.IRepository IShipmentData.Shipments => Get<ShipmentRepository>();
    Shipment.Domain.Entities.Item.IRepository IShipmentData.ShipmentItems => Get<ShipmentItemRepository>();
}
