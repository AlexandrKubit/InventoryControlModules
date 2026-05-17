namespace Receipt.Domain.Data;
using Core.Domain;
using Receipt.Domain.Entities;
using Directories.Contracts;

public interface IReceiptData : IData
{
    public Document.IRepository Receipts { get; }
    public Item.IRepository ReceiptItems { get; }
    public MeasureUnitContract.IMeasureUnitProjectionRepository MeasureUnitProjections { get; }
    public ResourceContract.IResourceProjectionRepository ResourceProjections { get; }
}
