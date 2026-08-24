namespace App.Commands.Handlers.Warehouse.Receipt.Delete;

using Common.Requests;
using Exchange.Commands.Warehouse.Receipt.Delete;
using System.Threading.Tasks;
using global::Receipt.Domain.Entities;
using global::Receipt.Domain.Data;
using App.Commands.Base;

[RequestRoute("/Warehouse/Receipt/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IReceiptData data, IUnitOfWork uow) : IRequestHandler<Request, Guid>
{
    System.Data.IsolationLevel IBaseRequestHandler.IsolationLevel => System.Data.IsolationLevel.ReadCommitted;

    public async Task<Guid> HandleAsync(Request request)
    {
        // необходимо из за горячей точки с редактированием
        await uow.AcquireLock(typeof(Document), request.Guid.ToString());
        await Document.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

