using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    /// <summary>Фильтрация курсов. Все параметры необязательные и комбинируются через AND.</summary>
    Task<IReadOnlyList<Course>> GetFilteredAsync(
        string? search, int? teacherId, string? department, int? minCredits, int? maxCredits);

    /// <summary>Курс вместе с преподавателем (только для чтения).</summary>
    Task<Course?> GetWithTeacherAsync(int id);

    /// <summary>Количество записанных студентов по каждому курсу (один GROUP BY-запрос).</summary>
    Task<IReadOnlyDictionary<int, int>> GetEnrollmentCountsAsync(IReadOnlyCollection<int> courseIds);
}
