namespace App.Commands.Handlers.Directories.Resource.ChangeCondition;

using Common.Requests;
using Exchange.Commands.Directories.Resource.ChangeCondition;
using global::Directories.Domain.Data;
using global::Directories.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Directories/Resource/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await data.Resources.EnsureByGuids([request.Guid]);
        var resource = data.Resources.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (resource.Condition == Resource.Conditions.Work)
            await Resource.ToArchiveRange([request.Guid], data);
        else
            await Resource.ToWorkRange([request.Guid], data);

        return resource.Guid;
    }
}

