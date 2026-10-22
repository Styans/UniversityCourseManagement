namespace UniversityApi.Repositories.Interfaces;

/// <summary>
/// Базовый обобщённый репозиторий: общие CRUD-операции для любой сущности.
/// Специфичные запросы объявляются в наследующих интерфейсах (IStudentRepository и т. д.).
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<IReadOnlyList<TEntity>> GetAllAsync();

    /// <summary>Возвращает отслеживаемую (tracked) сущность — её можно изменить и сохранить.</summary>
    Task<TEntity?> GetByIdAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);
}
