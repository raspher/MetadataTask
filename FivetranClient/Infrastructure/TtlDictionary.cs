using System.Diagnostics.CodeAnalysis;

namespace FivetranClient.Infrastructure;

public class TtlDictionary<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, (TValue Value, DateTime ExpirationTime)> _dictionary = new();

    public TValue GetOrAdd(TKey key, Func<TValue> valueFactory, TimeSpan ttl)
    {
        if (_dictionary.TryGetValue(key, out var entry))
        {
            if (entry.ExpirationTime > DateTime.UtcNow)
            {
                return entry.Value;
            }

            _dictionary.Remove(key);
        }

        var value = valueFactory();
        _dictionary[key] = (value, DateTime.UtcNow.Add(ttl));
        return value;
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        if (_dictionary.TryGetValue(key, out var entry) && entry.ExpirationTime > DateTime.UtcNow)
        {
            value = entry.Value;
            return value is not null;
        }

        value = default;
        return false;
    }
}