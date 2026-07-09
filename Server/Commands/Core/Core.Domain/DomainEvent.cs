namespace Core.Domain;

public class DomainEvent<TArg>
{
    private readonly Dictionary<Type, Func<TArg, Task>> dictionary = new();

    /// <summary>
    /// Важное ограничение: на одно событие дожен быть один обработчик от каждого типа
    /// но это и правильно, ведь на событие тип должен реагировать однозначно
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public async Task Invoke(TArg arg)
    {
        foreach (var handler in dictionary)
        {
            await handler.Value(arg);
        }
    }

    /// <summary>
    /// Важное ограничение: для нормальной работы мы можем подписать только статические методы типа
    /// </summary>
    /// <param name="handler"></param>
    public void Subscribe(Func<TArg, Task> handler)
    {
        if (handler != null && handler.Method.DeclaringType != null)
        {
            Type declaringType = handler.Method.DeclaringType;
            dictionary[declaringType] = handler;
        }
    }
}
