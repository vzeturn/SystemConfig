using Domain;

namespace Infrastructure;

public abstract class RepositoryBase<T> : IRepository<T> where T : AggregateRoot
{
    public abstract Task<T?> GetByIdAsync(Guid id);
    public abstract Task<IEnumerable<T>> GetAllAsync();
    public abstract Task AddAsync(T entity);
    public abstract Task UpdateAsync(T entity);
    public abstract Task DeleteAsync(T entity);
} 