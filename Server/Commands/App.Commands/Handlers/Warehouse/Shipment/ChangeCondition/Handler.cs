namespace App.Commands.Handlers.Warehouse.Shipment.ChangeCondition;

using Common.Requests;
using Exchange.Commands.Warehouse.Shipment.ChangeCondition;
using global::Shipment.Domain.Data;
using global::Shipment.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Warehouse/Shipment/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler(IShipmentData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await data.Shipments.EnsureByGuids([request.Guid]);

        var shipment = data.Shipments.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (shipment.Condition == Document.Conditions.Unsigned)
        {
            await Document.SignRange([shipment.Guid], data);
        }
        else
        {
            await Document.UnsignRange([shipment.Guid], data);
        }
        return shipment.Guid;
    }
}

