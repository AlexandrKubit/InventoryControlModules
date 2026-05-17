namespace Receipt.Infrastructure;
using Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Receipt.Domain.Entities;

public class ReceiptRepository : BaseRepository<Document>, Document.IRepository
{
    private Document Restore(Core.Infrastructure.Entities.Receipt receipt) =>
        Document.IRepository.Restore(receipt.Guid, receipt.Number, receipt.Date);

    public async Task EnsureByNumbers(HashSet<string> numbers)
    {
        var func = async (IEnumerable<string> args) =>
             await Context.Receipts
                 .Where(x => args.Contains(x.Number))
                 .Where(x => !LoadedGuids.Contains(x.Guid))
                 .ToDictionaryAsync(x => x.Guid, x => Restore(x));

        await LoadWithCacheAsync(numbers, func);
    }


    public override void Commit()
    {
        EntityCommitHelper.CommitEntities(
            dbSet: Context.Receipts,
            entities: Collection.Values,
            createMapDelegate: entity => new Core.Infrastructure.Entities.Receipt
            {
                Guid = entity.Guid,
                Number = entity.Number,
                Date = entity.Date.ToUniversalTime()
            },
            updateMapDelegate: (dbEntity, entity) =>
            {
                dbEntity.Number = entity.Number;
                dbEntity.Date = entity.Date.ToUniversalTime();
            }
        );
    }

    protected override async Task<Dictionary<Guid, Document>> GetFromDbByGuidsAsync(HashSet<Guid> guids)
    {
        return await Context.Receipts
            .Where(x => guids.Contains(x.Guid))
            .ToDictionaryAsync(x => x.Guid, x => Restore(x));
    }
}
