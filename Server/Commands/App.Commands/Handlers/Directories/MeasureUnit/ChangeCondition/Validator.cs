namespace App.Commands.Handlers.Directories.MeasureUnit.ChangeCondition;

using Exchange.Commands.Directories.MeasureUnit.ChangeCondition;
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
