namespace Receipt.Domain.Data;

using Core.Domain;
using Receipt.Domain.Entities;

public interface IReceiptData : IData
{
    public Document.IRepository Receipts { get; }
    public Item.IRepository ReceiptItems { get; }
}
