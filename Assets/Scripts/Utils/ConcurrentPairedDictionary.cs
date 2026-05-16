using System.Collections.Concurrent;

public sealed class ConcurrentPairedDictionary<T1, T2>
{
    private ConcurrentDictionary<T1, T2> _directDictionary = new ConcurrentDictionary<T1, T2>();
    private ConcurrentDictionary<T2, T1> _inverseDictionary = new ConcurrentDictionary<T2, T1>();

    public bool Add(T1 value, T2 value2)
    {
        _directDictionary.TryAdd(value, value2);
        return _inverseDictionary.TryAdd(value2, value);
    }

    public bool TryGet(T1 value, out T2 val)
    {
        return _directDictionary.TryGetValue(value, out val);
    }

    public bool TryGet(T2 value, out T1 val)
    {
        return _inverseDictionary.TryGetValue(value, out val);
    }

    public bool Contains(T1 value)
    {
        return _directDictionary.ContainsKey(value);
    }

    public bool Contains(T2 value)
    {
        return _inverseDictionary.ContainsKey(value);
    }
}