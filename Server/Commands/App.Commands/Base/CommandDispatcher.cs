namespace App.Commands.Base;
using Common.Requests;
using Exchange;

public class CommandDispatcher : ICommandDispatcher
{
    public async Task<object> HandleAsync(IBaseRequestHandler handler, IBaseRequest request, IServiceProvider provider)
    {
        var uow = (IUnitOfWork)provider.GetService(typeof(IUnitOfWork));
        return await PrivateHandleAsync(handler, request, uow);
    }

    private static async Task<object> PrivateHandleAsync(IBaseRequestHandler handler, IBaseRequest request, IUnitOfWork uow)
    {
        for (int retry = 0; retry < 5; retry++) // Разрешим максимум 5 попыток.
        {
            try
            {
                // передаем уровень изоляции транзакций
                await uow.InitializeAsync(handler.IsolationLevel);
                var result = await handler.BaseHandleAsync(request);
                await uow.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await uow.RollbackAsync();
                if (uow.IsTransientConcurrencyException(ex))
                    await Task.Delay(retry * 1000 + Random.Shared.Next(0, 500));
                else
                    throw; // Пробрасываем другие исключения.
            }
        }

        throw new Exception("Достигнут лимит повторов транзакций.");
    }
}
