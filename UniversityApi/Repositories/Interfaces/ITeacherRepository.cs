using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ITeacherRepository : IRepository<Teacher>
{
    /// <summary>Список преподавателей с необязательным фильтром по кафедре.</summary>
    Task<IReadOnlyList<Teacher>> GetAllAsync(string? department);

    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    /// <summary>Есть ли у преподавателя закреплённые курсы (тогда удалять его нельзя).</summary>
    Task<bool> HasCoursesAsync(int teacherId);
}
