using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<Course>> GetFilteredAsync(
        string? search, int? teacherId, string? department, int? minCredits, int? maxCredits)
    {
        // IQueryable собирается по частям; SQL-запрос выполняется один раз в ToListAsync()
        IQueryable<Course> query = Set.AsNoTracking().Include(c => c.Teacher);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        if (teacherId.HasValue)
            query = query.Where(c => c.TeacherId == teacherId.Value);

        if (!string.IsNullOrWhiteSpace(department))
        {
            var dep = department.Trim().ToLower();
            query = query.Where(c => c.Teacher.Department.ToLower().Contains(dep));
        }

        if (minCredits.HasValue)
            query = query.Where(c => c.Credits >= minCredits.Value);

        if (maxCredits.HasValue)
            query = query.Where(c => c.Credits <= maxCredits.Value);

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Course?> GetWithTeacherAsync(int id)
        => await Set.AsNoTracking()
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);
}
