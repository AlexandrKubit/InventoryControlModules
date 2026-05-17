namespace App.Commands.Handlers.Directories.Client.ChangeCondition;

using Exchange.Commands.Directories.Client.ChangeCondition;
using System.Threading.Tasks;
using Common.Requests;
using global::Directories.Domain.Entities;
using global::Directories.Domain.Data;

[RequestRoute("/Directories/Client/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await data.Clients.EnsureByGuids([request.Guid]);
        var client = data.Clients.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (client.Condition == Client.Conditions.Work)
            await Client.ToArchiveRange([request.Guid], data);
        else
            await Client.ToWorkRange([request.Guid], data);

        return client.Guid;
    }
}

