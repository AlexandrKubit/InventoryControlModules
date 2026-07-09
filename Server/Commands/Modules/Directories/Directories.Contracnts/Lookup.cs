namespace Directories.Contracts;

using Core.Domain;
using Directories.Domain.Data;
using Directories.Domain.Entities;

public static class Lookup
{
    public record ClientDTO(Guid Guid);

    public async static Task<List<ClientDTO>> GetActiveClientsAsync(HashSet<Guid> guids, IData data)
    {
        var cData = (IDirectoriesData)data;
        await cData.Clients.EnsureByGuids(guids);

        return cData.Clients.List
            .Where(x => guids.Contains(x.Guid))
            .Where(x => x.Condition == Client.Conditions.Work)
            .Select(x => new ClientDTO(x.Guid))
            .ToList();
    }

    public record MeasureUnitDTO(Guid Guid);

    public async static Task<List<MeasureUnitDTO>> GetActiveMeasureUnitsAsync(HashSet<Guid> guids, IData data)
    {
        var cData = (IDirectoriesData)data;
        await cData.MeasureUnits.EnsureByGuids(guids);

        return cData.MeasureUnits.List
            .Where(x => guids.Contains(x.Guid))
            .Where(x => x.Condition == MeasureUnit.Conditions.Work)
            .Select(x => new MeasureUnitDTO(x.Guid))
            .ToList();
    }

    public record ResourceDTO(Guid Guid);

    public async static Task<List<ResourceDTO>> GetActiveResourcesAsync(HashSet<Guid> guids, IData data)
    {
        var cData = (IDirectoriesData)data;
        await cData.Resources.EnsureByGuids(guids);

        return cData.Resources.List
            .Where(x => guids.Contains(x.Guid))
            .Where(x => x.Condition == Resource.Conditions.Work)
            .Select(x => new ResourceDTO(x.Guid))
            .ToList();
    }
}
