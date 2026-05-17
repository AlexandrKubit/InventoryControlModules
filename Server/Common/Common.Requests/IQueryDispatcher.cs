namespace Common.Requests;
using Exchange;

public interface IQueryDispatcher
{
    Task<object> HandleAsync(IBaseRequestHandler handler, IBaseRequest request, IServiceProvider provider);
}
