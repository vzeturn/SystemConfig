namespace Infrastructure;

public abstract class CacheBase<TKey, TValue>
{
    public abstract TValue? Get(TKey key);
    public abstract void Set(TKey key, TValue value);
    public abstract void Remove(TKey key);
} 