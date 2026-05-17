namespace App.Commands.Handlers.Directories.Client.Delete;

using Common.Requests;
using Exchange.Commands.Directories.Client.Delete;
using global::Directories.Domain.Entities;
using global::Directories.Domain.Data;
using System.Threading.Tasks;

[RequestRoute("/Directories/Client/Delete", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await Client.DeleteRange([request.Guid], data);
        return request.Guid;
    }
}

