using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class TeacherRepository : Repository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<Teacher>> GetAllAsync(string? department)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(department))
        {
            var term = department.Trim().ToLower();
            query = query.Where(t => t.Department.ToLower().Contains(term));
        }

        return await query
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var normalized = email.Trim().ToLower();
        return await Set.AnyAsync(t => t.Email == normalized && (excludeId == null || t.Id != excludeId));
    }

    public async Task<bool> HasCoursesAsync(int teacherId)
        => await Db.Courses.AnyAsync(c => c.TeacherId == teacherId);
}
