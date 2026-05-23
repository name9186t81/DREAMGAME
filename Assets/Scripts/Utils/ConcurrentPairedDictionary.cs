using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public sealed class ConcurrentPairedDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>
{
    private ConcurrentDictionary<T1, T2> _directDictionary = new ConcurrentDictionary<T1, T2>();
    private ConcurrentDictionary<T2, T1> _inverseDictionary = new ConcurrentDictionary<T2, T1>();

    public T1 this[T2 index]
    {
        get
        {
            return _inverseDictionary[index];
        }
        set
        {
            _inverseDictionary[index] = value;
        }
    }

    public T2 this[T1 index]
    {
        get
        {
            return _directDictionary[index];
        }
        set
        {
            _directDictionary[index] = value;
        }
    }

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

    public bool Remove(T1 value)
    {
        if(_directDictionary.TryRemove(value, out var t2))
        {
            return _inverseDictionary.TryRemove(t2, out _);
        }

        return false;
    }

    public bool Remove(T2 value)
    {
        if (_inverseDictionary.TryRemove(value, out var t1))
        {
            return _directDictionary.TryRemove(t1, out _);
        }

        return false;
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
    {
        return _directDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}