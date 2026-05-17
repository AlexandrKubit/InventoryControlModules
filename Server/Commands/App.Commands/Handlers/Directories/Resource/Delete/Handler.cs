namespace App.Commands.Handlers.Directories.Resource.Delete;

using Common.Requests;
using Exchange.Commands.Directories.Resource.Delete;
using global::Directories.Domain.Data;
using global::Directories.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Resource.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

