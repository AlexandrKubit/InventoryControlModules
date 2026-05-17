namespace Common.DI;
using App.Commands.Base;
using App.Queries.Base;
using Balance.Domain.Data;
using Common.Requests;
using Core.Data;
using Core.Domain;
using Directories.Domain.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Receipt.Domain.Data;
using Shipment.Domain.Data;

public static class ServiceCollectionExtensions
{
    public static void AddQueries(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Connection.SetConnectionString(connectionString);
        builder.Services.AddScoped<IQueryDispatcher>(sp => new QueryDispatcher());
    }

    public static void AddCommands(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICommandDispatcher>(sp => new CommandDispatcher());
    }

    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddScoped<UnitOfWork>(serviceProvider => new UnitOfWork(serviceProvider, connectionString));
        builder.Services.AddScoped<IData>(sp => sp.GetService<UnitOfWork>());
        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetService<UnitOfWork>());
        builder.Services.AddScoped<IBalanceData>(sp => sp.GetService<UnitOfWork>());
        builder.Services.AddScoped<IDirectoriesData>(sp => sp.GetService<UnitOfWork>());
        builder.Services.AddScoped<IReceiptData>(sp => sp.GetService<UnitOfWork>());
        builder.Services.AddScoped<IShipmentData>(sp => sp.GetService<UnitOfWork>());
    }
}
