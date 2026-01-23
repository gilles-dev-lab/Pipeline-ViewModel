using System;
using System.Collections.Concurrent;

/// <summary>
/// C’est un contenant de données pour partager les résultats des étapes (IStep) entre elles
/// (à la place de transmettre le VM)
/// </summary>
public sealed class BuildContext
{
    private readonly ConcurrentDictionary<Type, object> _map = new ConcurrentDictionary<Type, object>();

    public void Set<T>(T value) where T : class
    {
        _map[typeof(T)] = value;
    }

    public void Set(Type type, object value)
    {
        if (value != null && !type.IsInstanceOfType(value))
            throw new InvalidOperationException(
                $"Produced value of type {value.GetType()} is not assignable to {type}");
        _map[type] = value;
    }


    public T Get<T>() where T : class
    {
        object v;
        return _map.TryGetValue(typeof(T), out v) ? (T)v : null;
    }

    public bool TryGet<T>(out T value) where T : class
    {
        object v;
        if (_map.TryGetValue(typeof(T), out v))
        {
            value = (T)v;
            return true;
        }
        value = null;
        return false;
    }
}