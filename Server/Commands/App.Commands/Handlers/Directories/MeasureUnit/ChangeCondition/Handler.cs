namespace App.Commands.Handlers.Directories.MeasureUnit.ChangeCondition;

using Common.Requests;
using Exchange.Commands.Directories.MeasureUnit.ChangeCondition;
using global::Directories.Domain.Data;
using global::Directories.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Directories/MeasureUnit/ChangeCondition", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        await data.MeasureUnits.EnsureByGuids([request.Guid]);
        var unit = data.MeasureUnits.List.FirstOrDefault(x => x.Guid == request.Guid);

        if (unit.Condition == MeasureUnit.Conditions.Work)
            await MeasureUnit.ToArchiveRange([request.Guid], data);
        else
            await MeasureUnit.ToWorkRange([request.Guid], data);

        return unit.Guid;
    }
}

