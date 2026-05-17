namespace App.Commands.Handlers.Directories.MeasureUnit.Delete;

using Common.Requests;
using Exchange.Commands.Directories.MeasureUnit.Delete;
using global::Directories.Domain.Data;
using global::Directories.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await MeasureUnit.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

