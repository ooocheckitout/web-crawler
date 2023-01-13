namespace common;

public class PoolItem<T> : IDisposable
{
    T? Value { get; }
    Action<PoolItem<T>> DisposeAction { get; }

    public PoolItem(T value, Action<PoolItem<T>> disposeAction)
    {
        Value = value;
        DisposeAction = disposeAction;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    public static implicit operator T(PoolItem<T> item) => item.Value;

    public void Dispose() => DisposeAction(this);
}
