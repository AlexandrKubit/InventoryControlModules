namespace Receipt.Domain;
using Receipt.Domain.Entities;

public static class ContractsInitializer
{
    public static void Initialize()
    {
        Item.InitializeContracts();
    }
}
