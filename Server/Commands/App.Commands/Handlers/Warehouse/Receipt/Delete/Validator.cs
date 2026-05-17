namespace App.Commands.Handlers.Warehouse.Receipt.Delete;

using Exchange.Commands.Warehouse.Receipt.Delete;
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
