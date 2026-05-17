namespace App.Commands.Handlers.Warehouse.Shipment.Delete;

using Common.Requests;
using Exchange.Commands.Warehouse.Shipment.Delete;
using global::Shipment.Domain.Data;
using global::Shipment.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IShipmentData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Document.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

