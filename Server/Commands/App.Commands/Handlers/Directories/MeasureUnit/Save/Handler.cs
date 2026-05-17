namespace App.Commands.Handlers.Directories.MeasureUnit.Save;

using Common.Requests;
using Exchange.Commands.Directories.MeasureUnit.Save;
using global::Directories.Domain.Data;
using global::Directories.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/Save", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        if (request.Guid == Guid.Empty)
        {
            var units = await MeasureUnit.CreateRange([request.Name], data);
            return units.First().Guid;
        }
        else
        {
            await MeasureUnit.UpdateRange([new MeasureUnit.UpdateArg(request.Guid, request.Name)], data);
            return request.Guid;
        }
    }
}

