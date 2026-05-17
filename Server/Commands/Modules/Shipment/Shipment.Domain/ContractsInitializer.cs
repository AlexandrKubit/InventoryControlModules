namespace Shipment.Domain;
using Shipment.Domain.Entities;

public static class ContractsInitializer
{
    public static void Initialize()
    {
        Document.InitializeContracts();
    }
}
