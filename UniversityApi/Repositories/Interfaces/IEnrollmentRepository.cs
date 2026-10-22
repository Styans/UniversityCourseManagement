using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<IReadOnlyList<Enrollment>> GetFilteredAsync(int? studentId, int? courseId);

    /// <summary>Запись вместе со студентом и курсом (только для чтения).</summary>
    Task<Enrollment?> GetWithDetailsAsync(int id);

    /// <summary>Записан ли уже студент на этот курс (бизнес-правило №3).</summary>
    Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
}
