namespace Balance.Domain.Data;

using Core.Domain;

public interface IBalanceData : IData
{
    public Entities.Balance.IRepository Balances { get; }
}
