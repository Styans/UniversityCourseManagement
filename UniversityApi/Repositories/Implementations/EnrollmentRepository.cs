using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<Enrollment>> GetFilteredAsync(int? studentId, int? courseId, int? teacherId = null)
    {
        IQueryable<Enrollment> query = Set.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course);

        if (studentId.HasValue)
            query = query.Where(e => e.StudentId == studentId.Value);

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        if (teacherId.HasValue)
            query = query.Where(e => e.Course.TeacherId == teacherId.Value);

        return await query.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<Enrollment?> GetWithDetailsAsync(int id)
        => await Set.AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        => await Set.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
}
