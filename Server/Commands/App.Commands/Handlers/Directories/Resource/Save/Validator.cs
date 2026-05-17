namespace App.Commands.Handlers.Directories.Resource.Save;

using Exchange.Commands.Directories.Resource.Save;
using Common.Exceptions;
using Common.Requests;

public class Validator : IRequestValidator<Request, Guid>
{
    public void Validate(Request request)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw new ValidationException("Не указано наименование");
    }
}
