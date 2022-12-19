using System.Collections.Generic;

public class DropOutStack<T> : List<T>
{
    readonly int _capacity;

    public DropOutStack(int capacity) : base()
    {
        _capacity = capacity;
    }

    public void Push(T item)
    {
        if (Count == _capacity)
            RemoveAt(0);

        Add(item);
    }

    public bool TryPop(out T result)
    {
        result = default;

        if (Count == 0) return false;

        var last = Count - 1;

        result = this[last];

        RemoveAt(last);

        return true;
    }
}