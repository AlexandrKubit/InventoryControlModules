namespace App.Queries.Base;
using Common.Requests;
using Exchange;
using System;


public class QueryDispatcher : IQueryDispatcher
{
    public async Task<object> HandleAsync(IBaseRequestHandler handler, IBaseRequest request, IServiceProvider provider)
    {
        return await handler.BaseHandleAsync(request);
    }
}
