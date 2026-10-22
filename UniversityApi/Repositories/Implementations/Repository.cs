using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

/// <summary>
/// Базовая реализация репозитория поверх EF Core.
/// DbContext приходит через конструктор (Dependency Injection), а не создаётся через new.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext Db;
    protected readonly DbSet<TEntity> Set;

    public Repository(ApplicationDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
        => await Set.AsNoTracking().ToListAsync();

    public virtual async Task<TEntity?> GetByIdAsync(int id)
        => await Set.FindAsync(id);

    public virtual async Task<bool> ExistsAsync(int id)
        => await Set.AnyAsync(e => EF.Property<int>(e, "Id") == id);

    public virtual async Task AddAsync(TEntity entity)
    {
        await Set.AddAsync(entity);
        await Db.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        Set.Update(entity);
        await Db.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        Set.Remove(entity);
        await Db.SaveChangesAsync();
    }
}
