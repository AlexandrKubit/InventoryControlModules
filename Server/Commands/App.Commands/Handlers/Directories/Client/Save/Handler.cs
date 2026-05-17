namespace App.Commands.Handlers.Directories.Client.Save;

using Common.Requests;
using Exchange.Commands.Directories.Client.Save;
using global::Directories.Domain.Data;
using global::Directories.Domain.Entities;
using System.Threading.Tasks;

[RequestRoute("/Directories/Client/Save", RequestRouteAttribute.Types.Command)]
public class Handler(IDirectoriesData data) : IRequestHandler<Request, Guid>
{
    public async Task<Guid> HandleAsync(Request request)
    {
        if (request.Guid == Guid.Empty)
        {
            // Создание нового клиента
            var arg = new Client.CreateArg(request.Name, request.Address);
            var clients = await Client.CreateRange([arg], data);
            return clients.First().Guid;
        }
        else
        {
            // Обновление существующего клиента
            var arg = new Client.UpdateArg(
                request.Guid,
                request.Name,
                request.Address
            );
            await Client.UpdateRange([arg], data);
            return request.Guid;
        }
    }
}

