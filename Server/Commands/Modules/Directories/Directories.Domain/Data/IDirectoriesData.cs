namespace Directories.Domain.Data;

using Core.Domain;
using Directories.Domain.Entities;

public interface IDirectoriesData : IData
{
    public Client.IRepository Clients { get; }
    public Resource.IRepository Resources { get; }
    public MeasureUnit.IRepository MeasureUnits { get; }
}
