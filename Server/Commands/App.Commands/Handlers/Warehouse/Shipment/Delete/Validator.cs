namespace App.Commands.Handlers.Warehouse.Shipment.Delete;

using Exchange.Commands.Warehouse.Shipment.Delete;
using Common.Exceptions;
using Common.Requests;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (request.Guid == Guid.Empty)
            throw new ValidationException("Неправильный идентификатор");
    }
}
