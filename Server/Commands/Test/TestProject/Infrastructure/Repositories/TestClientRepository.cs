using Directories.Contracts;
using Directories.Domain.Entities;
using TestProject.Infrastructure;
using static Directories.Contracts.ClientContract;
namespace Tests.Infrastructure;

internal class TestClientRepository : TestBaseRepository<Client>, Client.IRepository, ClientContract.IClientProjectionRepository
{
    public class ClientProjection : IClientProjection
    {
        public Guid Guid { get; }
        public Conditions Condition { get; }

        public ClientProjection(Client client)
        {
            Guid = client.Guid;
            Condition = (Conditions)client.Condition;
        }
    }
    IEnumerable<IClientProjection> IClientProjectionRepository.List => List.Select(x => new ClientProjection(x));

    // для интеграциооных тестов
    public override void InitData()
    {
        Add(Guid.NewGuid(), "Client 1", "Address 1", Client.Conditions.Work);
        Add(Guid.NewGuid(), "Client 2", "Address 2", Client.Conditions.Archive);
    }

    // для юнит тестов
    public void Add(Guid guid, string name, string address, Client.Conditions condition)
    {
        collection.Add(
            guid,
            Client.IRepository.Restore(guid, name, address, condition)
        );
    }

    public Task EnsureByGuids(HashSet<Guid> guids) => Task.CompletedTask;
    public Task EnsureByNames(HashSet<string> names) => Task.CompletedTask;
}