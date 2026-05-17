namespace Directories.Domain;
using Directories.Domain.Entities;

public static class ContractsInitializer
{
    public static void Initialize()
    {
        Client.InitializeContracts();
        MeasureUnit.InitializeContracts();
        Resource.InitializeContracts();
    }
}
