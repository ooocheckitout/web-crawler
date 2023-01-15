namespace common;

public class PoolItem<T> : IDisposable
{
    T Value { get; }
    public int Index { get; }
    Action<PoolItem<T>> DisposeAction { get; }

    public PoolItem(T value, int index, Action<PoolItem<T>> disposeAction)
    {
        Value = value;
        Index = index;
        DisposeAction = disposeAction;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator T(PoolItem<T> item) => item.Value;

    public void Dispose() => DisposeAction(this);
}
