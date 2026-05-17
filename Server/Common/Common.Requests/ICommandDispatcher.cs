namespace Common.Requests;
using Exchange;

public interface ICommandDispatcher
{
    Task<object> HandleAsync(IBaseRequestHandler handler, IBaseRequest request, IServiceProvider provider);
}
