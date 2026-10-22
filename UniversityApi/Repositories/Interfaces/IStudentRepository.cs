using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IStudentRepository : IRepository<Student>
{
    /// <summary>Список студентов с необязательным поиском по имени, фамилии или email.</summary>
    Task<IReadOnlyList<Student>> GetAllAsync(string? search);

    /// <summary>Студент вместе с записями на курсы, курсами и преподавателями.</summary>
    Task<Student?> GetWithCoursesAsync(int id);

    /// <summary>Занят ли email другим студентом (excludeId — исключить себя при обновлении).</summary>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
